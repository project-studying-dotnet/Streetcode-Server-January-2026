using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

// Alias to resolve naming conflict with the namespace
using SubtitleEntity = Streetcode.DAL.Entities.AdditionalContent.Subtitle;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Subtitle.GetAll;

public class GetAllSubtitlesHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetAllSubtitlesHandler _handler;

    public GetAllSubtitlesHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new GetAllSubtitlesHandler(
            _mockRepoWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var subtitles = new List<SubtitleEntity> { new() };
        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetAllAsync(null, null))
            .ReturnsAsync(subtitles);

        // Act
        var result = await _handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDataType()
    {
        // Arrange
        var subtitles = new List<SubtitleEntity>();
        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetAllAsync(null, null))
            .ReturnsAsync(subtitles);
        _mockMapper.Setup(m => m.Map<IEnumerable<SubtitleDTO>>(subtitles))
            .Returns(new List<SubtitleDTO>());

        // Act
        var result = await _handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().BeAssignableTo<IEnumerable<SubtitleDTO>>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCountOfItems()
    {
        // Arrange
        var subtitles = new List<SubtitleEntity> { new(), new() };
        var subtitleDtos = new List<SubtitleDTO> { new(), new() };

        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetAllAsync(null, null))
            .ReturnsAsync(subtitles);
        _mockMapper.Setup(m => m.Map<IEnumerable<SubtitleDTO>>(subtitles))
            .Returns(subtitleDtos);

        // Act
        var result = await _handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldMapEntitiesToDtosCorrectly()
    {
        // Arrange
        var subtitles = new List<SubtitleEntity> { new() { Id = 1 } };
        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetAllAsync(null, null))
            .ReturnsAsync(subtitles);

        // Act
        await _handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);

        // Assert
        _mockMapper.Verify(m => m.Map<IEnumerable<SubtitleDTO>>(subtitles), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDataIsNull()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetAllAsync(null, null))
            .ReturnsAsync((IEnumerable<SubtitleEntity>?)null);

        // Act
        var result = await _handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEntityNotFound()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetAllAsync(null, null))
            .ReturnsAsync((IEnumerable<SubtitleEntity>?)null);

        // Act
        var result = await _handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectErrorMessage_WhenSubtitlesNull()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetAllAsync(null, null))
            .ReturnsAsync((IEnumerable<SubtitleEntity>?)null);
        const string expectedError = "Cannot find any subtitles";

        // Act
        var result = await _handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedError);
    }
}