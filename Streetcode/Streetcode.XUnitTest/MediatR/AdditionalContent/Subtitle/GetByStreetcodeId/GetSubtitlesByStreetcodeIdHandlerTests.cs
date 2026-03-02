using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetByStreetcodeId;
using Streetcode.BLL.Mapping.AdditionalContent;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Subtitle;

public class GetSubtitlesByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetSubtitlesByStreetcodeIdHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new SubtitleProfile());
        });
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_SubtitleExists_ReturnsSuccessWithMappedSubtitle()
    {
        // Arrange
        int streetcodeId = 10;
        var subtitle = new Streetcode.DAL.Entities.AdditionalContent.Subtitle
        {
            Id = 1,
            StreetcodeId = streetcodeId,
            SubtitleText = "Found it"
        };
        var query = new GetSubtitlesByStreetcodeIdQuery(
            streetcodeId);

        _mockRepo.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Streetcode.DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            null))
            .ReturnsAsync(subtitle);

        var handler = new GetSubtitlesByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SubtitleText.Should().Be("Found it");
    }

    [Fact]
    public async Task Handle_SubtitleDoesNotExist_ReturnsSuccessWithNullValue()
    {
        // Arrange
        int streetcodeId = 10;
        var query = new GetSubtitlesByStreetcodeIdQuery(
            streetcodeId);

        _mockRepo.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Streetcode.DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            null))
            .ReturnsAsync((Streetcode.DAL.Entities.AdditionalContent.Subtitle?)null);

        var handler = new GetSubtitlesByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_RepositoryReturnsData_CorrectDataTypeReturned()
    {
        // Arrange
        var query = new GetSubtitlesByStreetcodeIdQuery(
            1);

        _mockRepo.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Streetcode.DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            null))
            .ReturnsAsync(new Streetcode.DAL.Entities.AdditionalContent.Subtitle());

        var handler = new GetSubtitlesByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Value.Should().BeOfType<SubtitleDTO>();
    }
}   