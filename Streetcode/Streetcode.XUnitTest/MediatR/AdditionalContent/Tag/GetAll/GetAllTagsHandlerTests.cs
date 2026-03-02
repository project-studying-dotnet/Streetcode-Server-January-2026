using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetAll;
using Streetcode.BLL.Mapping.AdditionalContent; 
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using Streetcode.BLL.DTO.AdditionalContent;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Tag;

public class GetAllTagsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetAllTagsHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();

        // Real Mapper Setup
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new TagProfile());
        });
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_TagsExist_ReturnsSuccessWithCorrectCountAndMappedData()
    {
        // Arrange
        var tags = new List<DAL.Entities.AdditionalContent.Tag>
        {
            new() { Id = 1, Title = "Historical" },
            new() { Id = 2, Title = "Art" }
        };

        _mockRepo.Setup(r => r.TagRepository.GetAllAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            null))
            .ReturnsAsync(tags);

        var handler = new GetAllTagsHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Use BeAssignableTo for collection interfaces
        result.Value.Should().BeAssignableTo<IEnumerable<TagDTO>>();
        result.Value.Count().Should().Be(2);
        result.Value.First().Title.Should().Be("Historical");
    }

    [Fact]
    public async Task Handle_TagsNotFound_ReturnsFailureAndLogsError()
    {
        // Arrange
        _mockRepo.Setup(r => r.TagRepository.GetAllAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            null))
            .ReturnsAsync((IEnumerable<DAL.Entities.AdditionalContent.Tag>?)null);

        var handler = new GetAllTagsHandler(_mockRepo.Object, _mapper, _mockLogger.Object);
        var query = new GetAllTagsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Cannot find any tags");
        _mockLogger.Verify(x => x.LogError(query, "Cannot find any tags"), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsSuccessWithEmptyCollection()
    {
        // Arrange
        _mockRepo.Setup(r => r.TagRepository.GetAllAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            null))
            .ReturnsAsync(new List<DAL.Entities.AdditionalContent.Tag>());

        var handler = new GetAllTagsHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}