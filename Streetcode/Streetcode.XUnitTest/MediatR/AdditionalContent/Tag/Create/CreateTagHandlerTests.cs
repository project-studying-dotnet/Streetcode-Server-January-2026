using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.Create;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Tag.Create;

public class CreateTagHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly CreateTagHandler _handler;

    public CreateTagHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new CreateTagHandler(
            _mockRepoWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTagIsCreated()
    {
        // Arrange
        var createTagDto = new CreateTagDTO { Title = "Test" };
        var tag = new DAL.Entities.AdditionalContent.Tag { Id = 1, Title = "Test" };

        _mockRepoWrapper.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(tag);

        // Act
        var result = await _handler.Handle(new CreateTagQuery(createTagDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDataType()
    {
        // Arrange
        var createTagDto = new CreateTagDTO();
        var tag = new DAL.Entities.AdditionalContent.Tag { Id = 1 };
        var tagDto = new TagDTO { Id = 1 };

        _mockRepoWrapper.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(tag);
        _mockMapper.Setup(m => m.Map<TagDTO>(tag))
            .Returns(tagDto);

        // Act
        var result = await _handler.Handle(new CreateTagQuery(createTagDto), CancellationToken.None);

        // Assert
        result.Value.Should().BeOfType<TagDTO>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCountOfItems_MeaningNotNull()
    {
        // Arrange
        var createTagDto = new CreateTagDTO();
        var tag = new DAL.Entities.AdditionalContent.Tag { Id = 1 };

        _mockRepoWrapper.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(tag);
        _mockMapper.Setup(m => m.Map<TagDTO>(tag)).Returns(new TagDTO());

        // Act
        var result = await _handler.Handle(new CreateTagQuery(createTagDto), CancellationToken.None);

        // Assert
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapCreatedEntityToDtoCorrectly()
    {
        // Arrange
        var createTagDto = new CreateTagDTO { Title = "New Tag" };
        var tag = new DAL.Entities.AdditionalContent.Tag { Id = 1, Title = "New Tag" };

        _mockRepoWrapper.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(tag);

        // Act
        await _handler.Handle(new CreateTagQuery(createTagDto), CancellationToken.None);

        // Assert
        _mockMapper.Verify(m => m.Map<TagDTO>(tag), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExceptionIsThrown()
    {
        // Arrange
        var createTagDto = new CreateTagDTO();
        _mockRepoWrapper.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(new DAL.Entities.AdditionalContent.Tag());

        _mockRepoWrapper.Setup(r => r.SaveChanges()).Throws(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(new CreateTagQuery(createTagDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectErrorMessage_WhenExceptionOccurs()
    {
        // Arrange
        var createTagDto = new CreateTagDTO();
        var exceptionMsg = "Persistence failed";

        _mockRepoWrapper.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(new DAL.Entities.AdditionalContent.Tag());
        _mockRepoWrapper.Setup(r => r.SaveChanges()).Throws(new Exception(exceptionMsg));

        // Act
        var result = await _handler.Handle(new CreateTagQuery(createTagDto), CancellationToken.None);

        // Assert
        result.Errors.First().Message.Should().Contain(exceptionMsg);
    }

    [Fact]
    public async Task Handle_ShouldLogErrorMessage_WhenExceptionOccurs()
    {
        // Arrange
        var createTagDto = new CreateTagDTO();
        var query = new CreateTagQuery(createTagDto);
        _mockRepoWrapper.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(new DAL.Entities.AdditionalContent.Tag());
        _mockRepoWrapper.Setup(r => r.SaveChanges()).Throws(new Exception("Log this error"));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockLogger.Verify(x => x.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
    }
}