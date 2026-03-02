using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.GetById;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetById;
using Streetcode.DAL.Entities.AdditionalContent.Subtitles;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Subtitle;

public class GetSubtitleByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetSubtitleByIdHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();

        // Real Mapper Setup
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_SubtitleExists_ReturnsSuccessWithCorrectMappedData()
    {
        // Arrange
        int testId = 1;
        var subtitle = new Subtitle { Id = testId, SubtitleText = "Sample Subtitle" };
        var query = new GetSubtitleByIdQuery(testId);

        _mockRepo.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Subtitle, bool>>>(),
            null))
            .ReturnsAsync(subtitle);

        var handler = new GetSubtitleByIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<SubtitleDTO>();
        result.Value.Id.Should().Be(testId);
        result.Value.SubtitleText.Should().Be("Sample Subtitle");
    }

    [Fact]
    public async Task Handle_SubtitleDoesNotExist_ReturnsFailureAndLogsError()
    {
        // Arrange
        int testId = 99;
        var query = new GetSubtitleByIdQuery(testId);

        _mockRepo.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Subtitle, bool>>>(),
            null))
            .ReturnsAsync((Subtitle?)null);

        var handler = new GetSubtitleByIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be($"Cannot find a subtitle with corresponding id: {testId}");
        _mockLogger.Verify(x => x.LogError(query, It.IsAny<string>()), Times.Once);
    }
}