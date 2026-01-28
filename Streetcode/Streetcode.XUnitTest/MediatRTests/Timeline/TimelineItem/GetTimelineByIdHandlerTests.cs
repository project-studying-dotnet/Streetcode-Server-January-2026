namespace Streetcode.XUnitTest.MediatRTests.Timeline.TimelineItem;

using AutoMapper;
using Moq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetById;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Timeline;
using Xunit;

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
        => new(
            repoWrapperMock.Object,
            mapper,
            loggerMock.Object
        );

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
                    IIncludableQueryable<TimelineItem, object>>>()
            ))
            .ReturnsAsync(entity);

        var query = new GetTimelineItemByIdQuery(1);

        // Act
        var result = await CreateHandler().Handle(
            query,
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
    }

    [Fact]
    public async Task Handle_ReturnsFail_WhenTimelineItemNotExists()
    {
        // Arrange
        timelineRepoMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItem, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItem>,
                    IIncludableQueryable<TimelineItem, object>>>()
            ))
            .ReturnsAsync((TimelineItem)null!);

        var query = new GetTimelineItemByIdQuery(99);

        // Act
        var result = await CreateHandler().Handle(
            query,
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(
            $"Cannot find a timeline item with corresponding id: {query.Id}",
            result.Errors[0].Message
        );

        loggerMock.Verify(
            l => l.LogError(
                query,
                It.IsAny<string>()
            ),
            Times.Once
        );
    }
}
