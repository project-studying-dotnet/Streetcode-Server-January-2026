using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetAll;
using Streetcode.BLL.Mapping.AdditionalContent;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
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

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new SubtitleProfile());
        });
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_SubtitlesExist_ReturnsSuccessWithCorrectTypeAndCount()
    {
        // Arrange
        var subtitles = new List<Streetcode.DAL.Entities.AdditionalContent.Subtitle>
        {
            new() { Id = 1, SubtitleText = "Subtitle 1" },
            new() { Id = 2, SubtitleText = "Subtitle 2" }
        };

        _mockRepo.Setup(r => r.SubtitleRepository.GetAllAsync(
            It.IsAny<Expression<Func<Streetcode.DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            null))
            .ReturnsAsync(subtitles);

        var handler = new GetAllSubtitlesHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(
            new GetAllSubtitlesQuery(),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<IEnumerable<SubtitleDTO>>();
        result.Value.Count().Should().Be(2);
        result.Value.First().SubtitleText.Should().Be("Subtitle 1");
    }

    [Fact]
    public async Task Handle_SubtitlesNotFound_ReturnsFailureAndLogsError()
    {
        // Arrange
        _mockRepo.Setup(r => r.SubtitleRepository.GetAllAsync(
            It.IsAny<Expression<Func<Streetcode.DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            null))
            .ReturnsAsync((IEnumerable<Streetcode.DAL.Entities.AdditionalContent.Subtitle>?)null);

        var handler = new GetAllSubtitlesHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        var query = new GetAllSubtitlesQuery();

        var expectedError = Messages.Error_EntitiesNotFound.Format(
            nameof(Streetcode.DAL.Entities.AdditionalContent.Subtitle));

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);

        _mockLogger.Verify(x => x.LogError(
            query,
            expectedError), Times.Once);
    }
}