using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.AdditionalContent;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using System.Linq.Expressions;
using Xunit;

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

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new TagProfile());
        });
        _mapper = config.CreateMapper();
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
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Tag>, IIncludableQueryable<DAL.Entities.AdditionalContent.Tag, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(tags);

        var handler = new GetAllTagsHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First().Title.Should().Be("Historical");
    }

    [Fact]
    public async Task Handle_TagsNotFound_ReturnsFailureAndLogsError()
    {
        // Arrange
        // FIX: Return an empty list instead of null to avoid ArgumentNullException in LINQ (.Any())
        // unless you specifically want to test null-handling logic in the handler.
        _mockRepo.Setup(r => r.TagRepository.GetAllAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Tag>, IIncludableQueryable<DAL.Entities.AdditionalContent.Tag, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(Enumerable.Empty<DAL.Entities.AdditionalContent.Tag>());

        var handler = new GetAllTagsHandler(_mockRepo.Object, _mapper, _mockLogger.Object);
        var query = new GetAllTagsQuery();
        var expectedError = Messages.Error_EntitiesNotFound.Format(nameof(DAL.Entities.AdditionalContent.Tag));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedError);
        _mockLogger.Verify(x => x.LogError(query, expectedError), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsFailureAsPerProjectStandard()
    {
        // Arrange
        _mockRepo.Setup(r => r.TagRepository.GetAllAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Tag>, IIncludableQueryable<DAL.Entities.AdditionalContent.Tag, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(new List<DAL.Entities.AdditionalContent.Tag>());

        var handler = new GetAllTagsHandler(_mockRepo.Object, _mapper, _mockLogger.Object);
        var expectedError = Messages.Error_EntitiesNotFound.Format(nameof(DAL.Entities.AdditionalContent.Tag));

        // Act
        var result = await handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(expectedError);
    }
}