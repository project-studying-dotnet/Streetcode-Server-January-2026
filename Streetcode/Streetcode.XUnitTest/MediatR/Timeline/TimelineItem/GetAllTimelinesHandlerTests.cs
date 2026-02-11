namespace Streetcode.XUnitTest.MediatR.Timeline.TimelineItem;

using AutoMapper;
using Moq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetAll;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Timeline;
using Xunit;

public class GetAllTimelineItemsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repoWrapperMock;
    private readonly Mock<ITimelineRepository> timelineRepoMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly IMapper mapper;

    public GetAllTimelineItemsHandlerTests()
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

    private GetAllTimelineItemsHandler CreateHandler()
        => new(
            repoWrapperMock.Object,
            mapper,
            loggerMock.Object
        );

    [Fact]
    public async Task Handle_ReturnsOk_WhenTimelineItemsExist()
    {
        //Arrange
        var entities = new List<TimelineItem>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };

        timelineRepoMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<TimelineItem, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItem>,
                    IIncludableQueryable<TimelineItem, object>>>()
            ))
            .ReturnsAsync(entities);

        // Act
        var result = await CreateHandler().Handle(
            new GetAllTimelineItemsQuery(),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
    }

    [Fact]
    public async Task Handle_ReturnsFail_WhenTimelineItemsAreNull()
    {
        //Arrange
        timelineRepoMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<TimelineItem, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItem>,
                    IIncludableQueryable<TimelineItem, object>>>()
            ))
            .ReturnsAsync((IEnumerable<TimelineItem>)null!);

        // Act
        var result = await CreateHandler().Handle(
            new GetAllTimelineItemsQuery(),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Cannot find any timelineItem", result.Errors[0].Message);

        loggerMock.Verify(
            l => l.LogError(
                It.IsAny<GetAllTimelineItemsQuery>(),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ReturnsOk_WhenTimelineItemsListIsEmpty()
    {
        //Arrange
        timelineRepoMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<TimelineItem, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItem>,
                    IIncludableQueryable<TimelineItem, object>>>()
                    ))
            .ReturnsAsync(new List<TimelineItem>());

        // Act
        var result = await CreateHandler().Handle(
            new GetAllTimelineItemsQuery(),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }
}
