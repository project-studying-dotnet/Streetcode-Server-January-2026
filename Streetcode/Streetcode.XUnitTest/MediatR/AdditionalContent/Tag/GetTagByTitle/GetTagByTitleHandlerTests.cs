using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.AdditionalContent;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByTitle;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
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

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new TagProfile());
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_TagExists_ReturnsSuccessWithCorrectData()
    {
        // Arrange
        string testTitle = "History";
        var query = new GetTagByTitleQuery(testTitle);
        var tagEntity = new DAL.Entities.AdditionalContent.Tag
        {
            Id = 1,
            Title = testTitle
        };

        _mockRepo.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Tag>, IIncludableQueryable<DAL.Entities.AdditionalContent.Tag, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(tagEntity);

        var handler = new GetTagByTitleHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(testTitle);
    }

    [Fact]
    public async Task Handle_TagDoesNotExist_ReturnsFailureAndLogsError()
    {
        // Arrange
        string testTitle = "NonExistent";
        var query = new GetTagByTitleQuery(testTitle);

        _mockRepo.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Tag>, IIncludableQueryable<DAL.Entities.AdditionalContent.Tag, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync((DAL.Entities.AdditionalContent.Tag?)null);

        var handler = new GetTagByTitleHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        string expectedError = $"Cannot find any tag with corresponding title: {testTitle}";

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(expectedError);
        _mockLogger.Verify(x => x.LogError(query, expectedError), Times.Once);
    }
}