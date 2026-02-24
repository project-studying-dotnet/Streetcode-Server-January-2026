using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.XUnitTest.MediatR.Timeline.TimelineItem.Get;

using AutoMapper;
using Moq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetById;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Timeline;
using Xunit;
using Streetcode.BLL.DTO.Timeline.TimelineItem;

public class GetTimelineItemByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repoWrapperMock;
    private readonly Mock<ITimelineRepository> timelineRepoMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly IMapper mapper;

    public GetTimelineItemByIdHandlerTests()
    {
        repoWrapperMock = new Mock<IRepositoryWrapper>();
        timelineRepoMock = new Mock<ITimelineRepository>();
        loggerMock = new Mock<ILoggerService>();

        repoWrapperMock
            .Setup(r => r.TimelineRepository)
            .Returns(timelineRepoMock.Object);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TimelineItem, TimelineItemDTO>();
        });

        mapper = mapperConfig.CreateMapper();
    }

    private GetTimelineItemByIdHandler CreateHandler()
        => new (
            repoWrapperMock.Object,
            mapper,
            loggerMock.Object);

    [Fact]
    public async Task Handle_ReturnsOk_WhenTimelineItemExists()
    {
        // Arrange
        var entity = new TimelineItem
        {
            Id = 1
        };

        timelineRepoMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItem, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItem>,
                    IIncludableQueryable<TimelineItem, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(entity);

        var query = new GetTimelineItemByIdQuery(1);

        // Act
        var result = await CreateHandler().Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
    }

    [Fact]
    public async Task Handle_ReturnsFail_WhenTimelineItemNotExists()
    {
        // Arrange
        var id = 99;
        var query = new GetTimelineItemByIdQuery(id);
        var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(TimelineItem), id);

        timelineRepoMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItem, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItem>,
                    IIncludableQueryable<TimelineItem, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((TimelineItem)null!);



        // Act
        var result = await CreateHandler().Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(errorMsg, result.Errors[0].Message);

        loggerMock.Verify(
            l => l.LogError(query, errorMsg),
            Times.Once);
    }
}
