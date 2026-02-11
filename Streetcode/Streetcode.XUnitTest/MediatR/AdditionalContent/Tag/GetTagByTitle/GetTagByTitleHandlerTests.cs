using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByStreetcodeId;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetTagByTitle;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

// Alias to resolve naming conflict between 'Tag' namespace and 'Tag' entity
using TagEntity = Streetcode.DAL.Entities.AdditionalContent.Tag;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Tag.GetTagByTitle;

public class GetTagByTitleHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetTagByTitleHandler _handler;

    public GetTagByTitleHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new GetTagByTitleHandler(
            _mockRepoWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var title = "History";
        var tag = new TagEntity { Id = 1, Title = title };
        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync(tag);

        // Act
        var result = await _handler.Handle(new GetTagByTitleQuery(title), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDataType()
    {
        // Arrange
        var title = "Culture";
        var tag = new TagEntity { Id = 1, Title = title };
        var tagDto = new TagDTO { Id = 1, Title = title };

        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync(tag);

        _mockMapper.Setup(m => m.Map<TagDTO>(tag))
            .Returns(tagDto);

        // Act
        var result = await _handler.Handle(new GetTagByTitleQuery(title), CancellationToken.None);

        // Assert
        result.Value.Should().BeOfType<TagDTO>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCountOfItems_MeaningNotNull()
    {
        // Arrange
        var title = "Art";
        var tag = new TagEntity { Id = 1, Title = title };
        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync(tag);

        _mockMapper.Setup(m => m.Map<TagDTO>(tag)).Returns(new TagDTO());

        // Act
        var result = await _handler.Handle(new GetTagByTitleQuery(title), CancellationToken.None);

        // Assert
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapEntityToDtoCorrectly()
    {
        // Arrange
        var title = "Science";
        var tag = new TagEntity { Id = 1, Title = title };
        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync(tag);

        // Act
        await _handler.Handle(new GetTagByTitleQuery(title), CancellationToken.None);

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
        var result = await _handler.Handle(new GetTagByTitleQuery("NonExistent"), CancellationToken.None);

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
        var result = await _handler.Handle(new GetTagByTitleQuery("Empty"), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectErrorMessage_WhenTagNotFound()
    {
        // Arrange
        string title = "MissingTitle";
        _mockRepoWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<TagEntity, bool>>>(), null))
            .ReturnsAsync((TagEntity?)null);

        var expectedError = $"Cannot find any tag by the title: {title}";

        // Act
        var result = await _handler.Handle(new GetTagByTitleQuery(title), CancellationToken.None);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedError);
    }
}