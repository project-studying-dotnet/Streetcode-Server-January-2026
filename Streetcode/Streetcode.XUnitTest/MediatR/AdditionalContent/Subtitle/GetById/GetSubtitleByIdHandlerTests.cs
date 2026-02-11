using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.GetById;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

// Alias to resolve conflict between 'Subtitle' namespace and 'Subtitle' entity class
using SubtitleEntity = Streetcode.DAL.Entities.AdditionalContent.Subtitle;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Subtitle.GetById;

public class GetSubtitleByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> mockRepoWrapper;
    private readonly Mock<IMapper> mockMapper;
    private readonly Mock<ILoggerService> mockLogger;
    private readonly GetSubtitleByIdHandler handler;

    public GetSubtitleByIdHandlerTests()
    {
        this.mockRepoWrapper = new Mock<IRepositoryWrapper>();
        this.mockMapper = new Mock<IMapper>();
        this.mockLogger = new Mock<ILoggerService>();

        this.handler = new GetSubtitleByIdHandler(
            this.mockRepoWrapper.Object,
            this.mockMapper.Object,
            this.mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var subtitle = new SubtitleEntity { Id = 1 };
        this.mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync(subtitle);

        // Act
        var result = await this.handler.Handle(new GetSubtitleByIdQuery(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDataType()
    {
        // Arrange
        var subtitle = new SubtitleEntity { Id = 1 };
        var subtitleDto = new SubtitleDTO { Id = 1 };

        this.mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync(subtitle);

        this.mockMapper.Setup(m => m.Map<SubtitleDTO>(subtitle))
            .Returns(subtitleDto);

        // Act
        var result = await this.handler.Handle(new GetSubtitleByIdQuery(1), CancellationToken.None);

        // Assert
        result.Value.Should().BeOfType<SubtitleDTO>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCountOfItems_MeaningNotNull()
    {
        // Arrange
        var subtitle = new SubtitleEntity { Id = 1 };
        this.mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync(subtitle);

        this.mockMapper.Setup(m => m.Map<SubtitleDTO>(subtitle))
            .Returns(new SubtitleDTO());

        // Act
        var result = await this.handler.Handle(new GetSubtitleByIdQuery(1), CancellationToken.None);

        // Assert
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapEntityToDtoCorrectly()
    {
        // Arrange
        var subtitle = new SubtitleEntity { Id = 1 };
        this.mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync(subtitle);

        // Act
        await this.handler.Handle(new GetSubtitleByIdQuery(1), CancellationToken.None);

        // Assert
        this.mockMapper.Verify(m => m.Map<SubtitleDTO>(subtitle), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDataIsNull()
    {
        // Arrange
        this.mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync((SubtitleEntity?)null);

        // Act
        var result = await this.handler.Handle(new GetSubtitleByIdQuery(1), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEntityNotFound()
    {
        // Arrange
        this.mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync((SubtitleEntity?)null);

        // Act
        var result = await this.handler.Handle(new GetSubtitleByIdQuery(1), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectErrorMessage_WhenSubtitleNotFound()
    {
        // Arrange
        int id = 1;
        this.mockRepoWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SubtitleEntity, bool>>>(), null))
            .ReturnsAsync((SubtitleEntity?)null);

        var expectedError = $"Cannot find a subtitle with corresponding id: {id}";

        // Act
        var result = await this.handler.Handle(new GetSubtitleByIdQuery(id), CancellationToken.None);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedError);
    }
}