using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.AdditionalContent.Tag; 
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.Create;
using Streetcode.BLL.Mapping.AdditionalContent;
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

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new TagProfile());
            cfg.CreateMap<CreateTagDTO, DAL.Entities.AdditionalContent.Tag>();
        });
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessAndMappedTag()
    {
        // Arrange
        var createTagDto = new CreateTagDTO { Title = "Test Tag" };
        var query = new CreateTagQuery(createTagDto);
        var createdTagFromDb = new DAL.Entities.AdditionalContent.Tag { Id = 1, Title = "Test Tag" };

        _mockRepo.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(createdTagFromDb);

        _mockRepo.Setup(r => r.SaveChanges()).Returns(1);

        var handler = new CreateTagHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<TagDTO>();
        result.Value.Title.Should().Be("Test Tag");
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsCreateAsyncWithCorrectData()
    {
        // Arrange
        var createTagDto = new CreateTagDTO { Title = "New Unique Tag" };
        var query = new CreateTagQuery(createTagDto);

        _mockRepo.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
            .ReturnsAsync(new DAL.Entities.AdditionalContent.Tag());

        var handler = new CreateTagHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepo.Verify(r => r.TagRepository.CreateAsync(It.Is<DAL.Entities.AdditionalContent.Tag>(t => t.Title == "New Unique Tag")), Times.Once);
    }
}