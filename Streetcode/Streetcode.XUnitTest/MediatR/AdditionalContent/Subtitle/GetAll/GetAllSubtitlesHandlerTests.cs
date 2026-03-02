using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetAll;
using Streetcode.DAL.Entities.AdditionalContent.Subtitles;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Subtitle;

public class GetAllSubtitlesHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetAllSubtitlesHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();

        // Real Mapper Setup
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_SubtitlesExist_ReturnsSuccessWithCorrectTypeAndCount()
    {
        // Arrange
        var subtitles = new List<Subtitle>
        {
            new() { Id = 1, SubtitleText = "Subtitle 1" },
            new() { Id = 2, SubtitleText = "Subtitle 2" }
        };

        _mockRepo.Setup(r => r.SubtitleRepository.GetAllAsync(
            It.IsAny<Expression<Func<Subtitle, bool>>>(),
            null))
            .ReturnsAsync(subtitles);

        var handler = new GetAllSubtitlesHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<List<SubtitleDTO>>();
        result.Value.Count().Should().Be(2);
        result.Value.First().SubtitleText.Should().Be("Subtitle 1");
    }

    [Fact]
    public async Task Handle_SubtitlesNotFound_ReturnsFailureAndLogsError()
    {
        // Arrange
        _mockRepo.Setup(r => r.SubtitleRepository.GetAllAsync(
            It.IsAny<Expression<Func<Subtitle, bool>>>(),
            null))
            .ReturnsAsync((IEnumerable<Subtitle>?)null);

        var handler = new GetAllSubtitlesHandler(_mockRepo.Object, _mapper, _mockLogger.Object);
        var query = new GetAllSubtitlesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Cannot find any subtitles");
        _mockLogger.Verify(x => x.LogError(query, "Cannot find any subtitles"), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsSuccessWithZeroCount()
    {
        // Arrange
        _mockRepo.Setup(r => r.SubtitleRepository.GetAllAsync(
            It.IsAny<Expression<Func<Subtitle, bool>>>(),
            null))
            .ReturnsAsync(new List<Subtitle>());

        var handler = new GetAllSubtitlesHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}