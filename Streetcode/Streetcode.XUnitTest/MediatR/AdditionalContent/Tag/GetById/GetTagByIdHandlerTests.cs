using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetById;
using Streetcode.BLL.Mapping.AdditionalContent; 
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Tag;

public class GetTagByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetTagByIdHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();

        // FIXED: Using TagProfile instead of the non-existent MappingProfile
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new TagProfile());
        });
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_TagExists_ReturnsSuccessWithMappedTag()
    {
        // Arrange
        int testId = 5;
        var tagEntity = new DAL.Entities.AdditionalContent.Tag { Id = testId, Title = "Culture" };
        var query = new GetTagByIdQuery(testId);

        _mockRepo.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            null))
            .ReturnsAsync(tagEntity);

        var handler = new GetTagByIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<TagDTO>();
        result.Value.Id.Should().Be(testId);
        result.Value.Title.Should().Be("Culture");
    }

    [Fact]
    public async Task Handle_TagDoesNotExist_ReturnsFailureAndLogsError()
    {
        // Arrange
        int testId = 999;
        var query = new GetTagByIdQuery(testId);
        string expectedError = $"Cannot find a Tag with corresponding id: {testId}";

        _mockRepo.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            null))
            .ReturnsAsync((DAL.Entities.AdditionalContent.Tag?)null);

        var handler = new GetTagByIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(expectedError);
        _mockLogger.Verify(x => x.LogError(query, expectedError), Times.Once);
    }
}