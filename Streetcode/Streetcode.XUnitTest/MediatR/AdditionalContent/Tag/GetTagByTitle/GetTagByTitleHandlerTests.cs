using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetTagByTitle;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Tag;

public class GetTagByTitleHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetTagByTitleHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();

        // Real Mapper Setup
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_TagExists_ReturnsSuccessWithCorrectData()
    {
        // Arrange
        string testTitle = "History";
        var query = new GetTagByTitleQuery(testTitle);
        var tagEntity = new DAL.Entities.AdditionalContent.Tag { Id = 1, Title = testTitle };

        _mockRepo.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            null))
            .ReturnsAsync(tagEntity);

        var handler = new GetTagByTitleHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<TagDTO>();
        result.Value.Title.Should().Be(testTitle);
    }

    [Fact]
    public async Task Handle_TagDoesNotExist_ReturnsFailureAndLogsError()
    {
        // Arrange
        string testTitle = "NonExistent";
        var query = new GetTagByTitleQuery(testTitle);
        string expectedError = $"Cannot find any tag by the title: {testTitle}";

        _mockRepo.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            null))
            .ReturnsAsync((DAL.Entities.AdditionalContent.Tag?)null);

        var handler = new GetTagByTitleHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(expectedError);
        _mockLogger.Verify(x => x.LogError(query, expectedError), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsCorrectDtoType()
    {
        // Arrange
        var query = new GetTagByTitleQuery("AnyTitle");
        _mockRepo.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null))
            .ReturnsAsync(new DAL.Entities.AdditionalContent.Tag());

        var handler = new GetTagByTitleHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeOfType<TagDTO>();
    }
}