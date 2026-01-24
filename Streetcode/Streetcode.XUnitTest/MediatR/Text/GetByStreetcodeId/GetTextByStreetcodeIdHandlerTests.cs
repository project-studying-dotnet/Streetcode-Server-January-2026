using AutoMapper;
using FluentAssertions;
using Moq;
using Org.BouncyCastle.Asn1.Ocsp;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Text;
using Streetcode.BLL.MediatR.Streetcode.Text.GetByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Text.GetByStreetcodeId
{
}

public class GetTextByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> mockRepoWrapper;
    private readonly Mock<IMapper> mockMapper;
    private readonly Mock<ILoggerService> mockLogger;
    private readonly Mock<ITextService> mockTextService;
    private readonly GetTextByStreetcodeIdHandler handler;

    public GetTextByStreetcodeIdHandlerTests()
    {
        mockRepoWrapper = new Mock<IRepositoryWrapper>();
        mockMapper = new Mock<IMapper>();
        mockLogger = new Mock<ILoggerService>();
        mockTextService = new Mock<ITextService>();

        handler = new GetTextByStreetcodeIdHandler(
            mockRepoWrapper.Object,
            mockMapper.Object,
            mockTextService.Object,
            mockLogger.Object);
    }

    [Fact]
    public async Task Handler_ShouldReturnOk_WhenTextExists()
    {
        // Arrange
        int streetcodeId = 123;
        var text = new Text { Id = 123, TextContent = "content", StreetcodeId = streetcodeId};
        var textDTO = new TextDTO { Id = 123, TextContent = "parsed" };
        var query = new GetTextByStreetcodeIdQuery(streetcodeId);

        mockRepoWrapper
            .Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Text, bool>>>(), null))
            .ReturnsAsync(text);

        mockTextService
            .Setup(t => t.AddTermsTag(It.IsAny<string>()))
            .ReturnsAsync("parsed");

        mockMapper
            .Setup(m => m.Map<TextDTO?>(text))
            .Returns(textDTO);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TextContent.Should().BeEquivalentTo("parsed");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public async Task Handler_ShouldReturnFail_IfTextNotExists(int streetcodeId)
    {
        // Arrange
        var query = new GetTextByStreetcodeIdQuery(streetcodeId);
        var errorMsg = $"Cannot find a transaction link by a streetcode id: {streetcodeId}, because such streetcode doesn`t exist";

        mockRepoWrapper
            .Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Text, bool>>>(), null))
            .ReturnsAsync((Text?)null);

        mockRepoWrapper
            .Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync((StreetcodeContent?)null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == errorMsg);
    }
}