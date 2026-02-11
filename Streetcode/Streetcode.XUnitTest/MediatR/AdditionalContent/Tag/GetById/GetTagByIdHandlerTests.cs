using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

// Alias to resolve naming conflict between 'Tag' namespace and 'Tag' entity
using TagEntity = Streetcode.DAL.Entities.AdditionalContent.Tag;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Tag.GetById;

public class GetTagByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetTagByIdHandler _handler;

    public GetTagByIdHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new GetTagByIdHandler(
            _mockRepoWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var tag = new TagEntity { Id = 1, Title = "Test" };
        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync(tag);

        // Act
        var result = await _handler.Handle(new GetTagByIdQuery(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDataType()
    {
        // Arrange
        var tag = new TagEntity { Id = 1 };
        var tagDto = new TagDTO { Id = 1 };

        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync(tag);

        _mockMapper.Setup(m => m.Map<TagDTO>(tag))
            .Returns(tagDto);

        // Act
        var result = await _handler.Handle(new GetTagByIdQuery(1), CancellationToken.None);

        // Assert
        result.Value.Should().BeOfType<TagDTO>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCountOfItems_MeaningNotNull()
    {
        // Arrange
        var tag = new TagEntity { Id = 1 };
        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync(tag);

        _mockMapper.Setup(m => m.Map<TagDTO>(tag)).Returns(new TagDTO());

        // Act
        var result = await _handler.Handle(new GetTagByIdQuery(1), CancellationToken.None);

        // Assert
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapEntityToDtoCorrectly()
    {
        // Arrange
        var tag = new TagEntity { Id = 1 };
        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync(tag);

        // Act
        await _handler.Handle(new GetTagByIdQuery(1), CancellationToken.None);

        // Assert
        _mockMapper.Verify(m => m.Map<TagDTO>(tag), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDataIsNull()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync((TagEntity?)null);

        // Act
        var result = await _handler.Handle(new GetTagByIdQuery(1), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEntityNotFound()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync((TagEntity?)null);

        // Act
        var result = await _handler.Handle(new GetTagByIdQuery(1), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectErrorMessage_WhenTagNotFound()
    {
        // Arrange
        int id = 1;
        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync((TagEntity?)null);

        var expectedError = $"Cannot find a Tag with corresponding id: {id}";

        // Act
        var result = await _handler.Handle(new GetTagByIdQuery(id), CancellationToken.None);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedError);
    }
}