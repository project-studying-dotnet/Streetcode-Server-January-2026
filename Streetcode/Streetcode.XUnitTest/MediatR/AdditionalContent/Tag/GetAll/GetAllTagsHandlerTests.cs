using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

// Alias to resolve naming conflict between 'Tag' namespace and 'Tag' entity
using TagEntity = Streetcode.DAL.Entities.AdditionalContent.Tag;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Tag.GetAll;

public class GetAllTagsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetAllTagsHandler _handler;

    public GetAllTagsHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new GetAllTagsHandler(
            _mockRepoWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var tags = new List<TagEntity> { new() { Id = 1, Title = "Test" } };
        _mockRepoWrapper.Setup(r => r.TagRepository.GetAllAsync(null, null))
            .ReturnsAsync(tags);

        // Act
        var result = await _handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDataType()
    {
        // Arrange
        var tags = new List<TagEntity>();
        _mockRepoWrapper.Setup(r => r.TagRepository.GetAllAsync(null, null))
            .ReturnsAsync(tags);
        _mockMapper.Setup(m => m.Map<IEnumerable<TagDTO>>(tags))
            .Returns(new List<TagDTO>());

        // Act
        var result = await _handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().BeAssignableTo<IEnumerable<TagDTO>>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCountOfItems()
    {
        // Arrange
        var tags = new List<TagEntity> { new(), new() };
        var tagDtos = new List<TagDTO> { new(), new() };

        _mockRepoWrapper.Setup(r => r.TagRepository.GetAllAsync(null, null))
            .ReturnsAsync(tags);
        _mockMapper.Setup(m => m.Map<IEnumerable<TagDTO>>(tags))
            .Returns(tagDtos);

        // Act
        var result = await _handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldMapEntitiesToDtosCorrectly()
    {
        // Arrange
        var tags = new List<TagEntity> { new() { Id = 1 } };
        _mockRepoWrapper.Setup(r => r.TagRepository.GetAllAsync(null, null))
            .ReturnsAsync(tags);

        // Act
        await _handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        _mockMapper.Verify(m => m.Map<IEnumerable<TagDTO>>(tags), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDataIsNull()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.TagRepository.GetAllAsync(null, null))
            .ReturnsAsync((IEnumerable<TagEntity>?)null);

        // Act
        var result = await _handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEntityNotFound()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.TagRepository.GetAllAsync(null, null))
            .ReturnsAsync((IEnumerable<TagEntity>?)null);

        // Act
        var result = await _handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectErrorMessage_WhenTagsNull()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.TagRepository.GetAllAsync(null, null))
            .ReturnsAsync((IEnumerable<TagEntity>?)null);
        const string expectedError = "Cannot find any tags";

        // Act
        var result = await _handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedError);
    }
}