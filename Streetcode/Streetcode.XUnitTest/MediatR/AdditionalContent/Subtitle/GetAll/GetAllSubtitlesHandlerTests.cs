using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.AdditionalContent;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetAll;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
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

        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_SubtitlesExist_ReturnsSuccessWithCorrectTypeAndCount()
    {
        // Arrange
        var subtitles = new List<DAL.Entities.AdditionalContent.Subtitle>
        {
            new() { Id = 1, SubtitleText = "Subtitle 1" },
            new() { Id = 2, SubtitleText = "Subtitle 2" }
        };

        _mockRepo.Setup(r => r.SubtitleRepository.GetAllAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Subtitle>, IIncludableQueryable<DAL.Entities.AdditionalContent.Subtitle, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(subtitles);

        var handler = new GetAllSubtitlesHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First().SubtitleText.Should().Be("Subtitle 1");
    }

    [Fact]
    public async Task Handle_SubtitlesNotFound_ReturnsFailureAndLogsError()
    {
        // Arrange
        // FIX: Returning an empty list to satisfy the .Any() check without crashing
        _mockRepo.Setup(r => r.SubtitleRepository.GetAllAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Subtitle>, IIncludableQueryable<DAL.Entities.AdditionalContent.Subtitle, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(new List<DAL.Entities.AdditionalContent.Subtitle>());

        var handler = new GetAllSubtitlesHandler(_mockRepo.Object, _mapper, _mockLogger.Object);
        var query = new GetAllSubtitlesQuery();
        var expectedError = Messages.Error_EntitiesNotFound.Format(nameof(DAL.Entities.AdditionalContent.Subtitle));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(expectedError);
        _mockLogger.Verify(x => x.LogError(query, expectedError), Times.Once);
    }
}