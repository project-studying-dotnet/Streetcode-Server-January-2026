using AutoMapper;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.AdditionalContent;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetByStreetcodeId;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
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
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_SubtitleExists_ReturnsSuccessWithMappedSubtitle()
    {
        // Arrange
        int streetcodeId = 10;
        var subtitle = new DAL.Entities.AdditionalContent.Subtitle
        {
            Id = 1,
            StreetcodeId = streetcodeId,
            SubtitleText = "Found it"
        };
        var query = new GetSubtitlesByStreetcodeIdQuery(streetcodeId);

        _mockRepo.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Subtitle>, IIncludableQueryable<DAL.Entities.AdditionalContent.Subtitle, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(subtitle);

        var handler = new GetSubtitlesByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SubtitleText.Should().Be("Found it");
    }

    [Fact]
    public async Task Handle_SubtitleDoesNotExist_ReturnsFailureAndLogsError()
    {
        // Arrange
        int streetcodeId = 10;
        var query = new GetSubtitlesByStreetcodeIdQuery(streetcodeId);

        _mockRepo.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Subtitle>, IIncludableQueryable<DAL.Entities.AdditionalContent.Subtitle, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync((DAL.Entities.AdditionalContent.Subtitle?)null);

        var handler = new GetSubtitlesByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        var expectedError = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
            nameof(DAL.Entities.AdditionalContent.Subtitle),
            streetcodeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        // FIXED: Handler returns False for IsSuccess when data is null
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);

        _mockLogger.Verify(x => x.LogError(query, expectedError), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryReturnsData_CorrectDataTypeReturned()
    {
        // Arrange
        var query = new GetSubtitlesByStreetcodeIdQuery(1);

        _mockRepo.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Subtitle>, IIncludableQueryable<DAL.Entities.AdditionalContent.Subtitle, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(new DAL.Entities.AdditionalContent.Subtitle());

        var handler = new GetSubtitlesByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeOfType<SubtitleDTO>();
    }
}