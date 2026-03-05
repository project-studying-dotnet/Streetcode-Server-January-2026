using AutoMapper;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetById;
using Streetcode.BLL.Mapping.AdditionalContent;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using System.Linq.Expressions;
using Xunit;
using Streetcode.BLL.MediatR.AdditionalContent.GetById;

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

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new SubtitleProfile());
        });

        // Correct way to initialize the mapper instance
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_SubtitleExists_ReturnsSuccessWithCorrectMappedData()
    {
        // Arrange
        int testId = 1;
        var subtitle = new DAL.Entities.AdditionalContent.Subtitle
        {
            Id = testId,
            SubtitleText = "Sample Subtitle"
        };
        var query = new GetSubtitleByIdQuery(testId);

        _mockRepo.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Subtitle>, IIncludableQueryable<DAL.Entities.AdditionalContent.Subtitle, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(subtitle);

        var handler = new GetSubtitleByIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

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
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Subtitle>, IIncludableQueryable<DAL.Entities.AdditionalContent.Subtitle, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync((DAL.Entities.AdditionalContent.Subtitle?)null);

        var handler = new GetSubtitleByIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        var expectedError = Messages.Error_EntityWithIdNotFound.Format(
            nameof(DAL.Entities.AdditionalContent.Subtitle),
            testId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);

        _mockLogger.Verify(x => x.LogError(query, expectedError), Times.Once);
    }
}