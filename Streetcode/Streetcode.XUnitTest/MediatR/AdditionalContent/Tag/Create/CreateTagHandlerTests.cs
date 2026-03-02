using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.Create;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Tag;

public class CreateTagHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public CreateTagHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();

        // Real Mapper Setup
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessAndMappedTag()
    {
        // Arrange
        var tagDto = new TagDTO { Title = "Test Tag" };
        var query = new CreateTagQuery(tagDto);
        var createdTag = new DAL.Entities.AdditionalContent.Tag { Id = 1, Title = "Test Tag" };

        _mockRepo.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(createdTag);

        _mockRepo.Setup(r => r.SaveChanges()).Returns(1);

        var handler = new CreateTagHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<TagDTO>();
        result.Value.Title.Should().Be("Test Tag");
        _mockRepo.Verify(r => r.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesThrowsException_ReturnsFailureAndLogsError()
    {
        // Arrange
        var tagDto = new TagDTO { Title = "Test Tag" };
        var query = new CreateTagQuery(tagDto);
        var exceptionMessage = "Database Error";

        _mockRepo.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(new DAL.Entities.AdditionalContent.Tag());

        _mockRepo.Setup(r => r.SaveChanges()).Throws(new Exception(exceptionMessage));

        var handler = new CreateTagHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain(exceptionMessage);
        _mockLogger.Verify(x => x.LogError(query, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsCreateAsyncWithCorrectData()
    {
        // Arrange
        var tagDto = new TagDTO { Title = "New Unique Tag" };
        var query = new CreateTagQuery(tagDto);

        _mockRepo.Setup(r => r.TagRepository.CreateAsync(It.Is<DAL.Entities.AdditionalContent.Tag>(t => t.Title == tagDto.Title)))
            .ReturnsAsync(new DAL.Entities.AdditionalContent.Tag());

        var handler = new CreateTagHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepo.Verify(r => r.TagRepository.CreateAsync(It.Is<DAL.Entities.AdditionalContent.Tag>(t => t.Title == "New Unique Tag")), Times.Once);
    }
}