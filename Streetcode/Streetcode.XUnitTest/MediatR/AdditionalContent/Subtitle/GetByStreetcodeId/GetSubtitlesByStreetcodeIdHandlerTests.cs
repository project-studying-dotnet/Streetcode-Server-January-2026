using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

// Alias to resolve the naming conflict between the namespace and the entity
using SubtitleEntity = Streetcode.DAL.Entities.AdditionalContent.Subtitle;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Subtitle.GetByStreetcodeId;

public class GetSubtitlesByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetSubtitlesByStreetcodeIdHandler _handler;

    public GetSubtitlesByStreetcodeIdHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new GetSubtitlesByStreetcodeIdHandler(
            _mockRepoWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var subtitle = new SubtitleEntity { StreetcodeId = 1 };
        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync(subtitle);

        // Act
        var result = await _handler.Handle(new GetSubtitlesByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDataType()
    {
        // Arrange
        var subtitle = new SubtitleEntity { StreetcodeId = 1 };
        var subtitleDto = new SubtitleDTO { StreetcodeId = 1 };

        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync(subtitle);

        _mockMapper.Setup(m => m.Map<SubtitleDTO>(subtitle))
            .Returns(subtitleDto);

        // Act
        var result = await _handler.Handle(new GetSubtitlesByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.Value.Should().BeOfType<SubtitleDTO>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCountOfItems_MeaningNotNull()
    {
        // Arrange
        var subtitle = new SubtitleEntity { StreetcodeId = 1 };
        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync(subtitle);

        _mockMapper.Setup(m => m.Map<SubtitleDTO>(subtitle))
            .Returns(new SubtitleDTO());

        // Act
        var result = await _handler.Handle(new GetSubtitlesByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapEntityToDtoCorrectly()
    {
        // Arrange
        var subtitle = new SubtitleEntity { StreetcodeId = 1 };
        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync(subtitle);

        // Act
        await _handler.Handle(new GetSubtitlesByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        _mockMapper.Verify(m => m.Map<SubtitleDTO>(subtitle), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_EvenWhenDataIsNull_DueToNullResult()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync((SubtitleEntity?)null);

        // Act
        var result = await _handler.Handle(new GetSubtitlesByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        // NullResult is often used to return a success state with a null value rather than a Fail state.
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}