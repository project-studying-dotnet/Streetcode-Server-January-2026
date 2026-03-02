namespace Streetcode.XUnitTest.MediatR.Timeline.TimelineItem.Update
{
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Timeline.TimelineItem;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Timeline;
    using Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;
    using Streetcode.DAL.Entities.Timeline;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode;
    using Streetcode.DAL.Repositories.Interfaces.Timeline;
    using Streetcode.Resources;
    using System.Linq.Expressions;
    using Xunit;
    using HistoricalContextEntity = Streetcode.DAL.Entities.Timeline.HistoricalContext;
    using StreetcodeEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeContent;
    using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;


    public class UpdateTimelineItemHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapper;
        private readonly Mock<ILoggerService> logger;
        private readonly Mock<ITimelineRepository> timelineRepository;
        private readonly Mock<IHistoricalContextRepository> historicalContextRepository;
        private readonly Mock<IStreetcodeRepository> streetcodeRepository;
        private readonly IMapper mapper;
        private readonly UpdateTimelineItemHandler handler;

        public UpdateTimelineItemHandlerTests()
        {
            this.repositoryWrapper = new Mock<IRepositoryWrapper>();
            this.logger = new Mock<ILoggerService>();
            this.timelineRepository = new Mock<ITimelineRepository>();
            this.historicalContextRepository = new Mock<IHistoricalContextRepository>();
            this.streetcodeRepository = new Mock<IStreetcodeRepository>();

            this.repositoryWrapper
                .Setup(r => r.TimelineRepository)
                .Returns(this.timelineRepository.Object);

            this.repositoryWrapper
                .Setup(r => r.HistoricalContextRepository)
                .Returns(this.historicalContextRepository.Object);

            this.repositoryWrapper
                .Setup(r => r.StreetcodeRepository)
                .Returns(this.streetcodeRepository.Object);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TimelineItemProfile>();
            });

            this.mapper = configuration.CreateMapper();

            this.handler = new UpdateTimelineItemHandler(
                this.repositoryWrapper.Object,
                this.logger.Object,
                this.mapper);
        }

        private void SetupSaveChangesMock(int result)
        {
            this.repositoryWrapper
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(result);
        }

        [Fact]
        public async Task ShouldReturnFail_WhenTimelineItemNotFound()
        {
            var updateDTO = new UpdateTimelineItemDTO
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                Date = DateTime.UtcNow,
                HistoricalContextIds = new List<int> { 1 },
                StreetcodeId = 1,
            };

            var command = new UpdateTimelineItemCommand(updateDTO);

            this.timelineRepository
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>,
                    IIncludableQueryable<TimelineItemEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((TimelineItemEntity?)null);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            var expectedErrorMsg = string.Format(Messages.Error_EntityWithIdNotFound, nameof(TimelineItem), updateDTO.Id);
            result.Errors.First().Message.Should().Be(expectedErrorMsg);
        }

        [Fact]
        public async Task ShouldReturnFail_WhenStreetcodeNotFound()
        {
            var updateDTO = new UpdateTimelineItemDTO
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                Date = DateTime.UtcNow,
                HistoricalContextIds = new List<int> { 1 },
                StreetcodeId = 1,
            };

            var command = new UpdateTimelineItemCommand(updateDTO);

            this.timelineRepository
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>,
                    IIncludableQueryable<TimelineItemEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new TimelineItemEntity() { Id = updateDTO.Id });

            this.streetcodeRepository
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeEntity>,
                    IIncludableQueryable<StreetcodeEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync((StreetcodeEntity?)null);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            var expectedErrorMsg = string.Format(Messages.Error_EntityWithIdNotFound, nameof(Streetcode), updateDTO.StreetcodeId);
            result.Errors.First().Message.Should().Be(expectedErrorMsg);
        }

        [Fact]
        public async Task ShouldReturnFail_WhenHistoricalContextsNotFound()
        {
            var updateDTO = new UpdateTimelineItemDTO
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                Date = DateTime.UtcNow,
                HistoricalContextIds = new List<int> { 1 },
                StreetcodeId = 1,
            };

            var command = new UpdateTimelineItemCommand(updateDTO);

            this.timelineRepository
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>,
                    IIncludableQueryable<TimelineItemEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new TimelineItemEntity() { Id = updateDTO.Id });

            this.streetcodeRepository
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeEntity>,
                    IIncludableQueryable<StreetcodeEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync(new StreetcodeEntity() { Id = updateDTO.StreetcodeId });

            this.historicalContextRepository
                .Setup(r => r.GetAllAsync(
                    It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<HistoricalContextEntity>,
                    IIncludableQueryable<HistoricalContextEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new List<HistoricalContextEntity>
                {
                     new () { Id = 2 },
                     new () { Id = 3 },
                });

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();

            var expectedErrorMsg = string.Format(Messages.Error_EntityWithIdNotFound, nameof(HistoricalContext), "1");
            result.Errors.First().Message.Should().Be(expectedErrorMsg);
        }

        [Fact]
        public async Task ShouldReturnOk_WhenDataIsValid()
        {
            var historicalContext = new HistoricalContextEntity 
            {
                Id = 1,
                Title = "Historical Context 1",
            };
            var updateDTO = new UpdateTimelineItemDTO
            {
                Id = 10,
                Title = "Updated Title",
                Description = "Updated Description",
                Date = DateTime.UtcNow,
                HistoricalContextIds = new List<int> { 1 },
                StreetcodeId = 1,
            };

            var command = new UpdateTimelineItemCommand(updateDTO);

            var existingItem = new TimelineItemEntity
            {
                Id = updateDTO.Id,
                HistoricalContextTimelines = new List<HistoricalContextTimeline>(),
            };

            var finalItem = new TimelineItemEntity
            {
                Id = updateDTO.Id,
                Title = updateDTO.Title,
                HistoricalContextTimelines = new List<HistoricalContextTimeline>
            {
                new ()
                {
                    HistoricalContextId = 1,
                    HistoricalContext = historicalContext,
                },
            },
            };

            this.timelineRepository
                .SetupSequence(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(existingItem)
                .ReturnsAsync(finalItem);

            this.streetcodeRepository
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeEntity>,
                    IIncludableQueryable<StreetcodeEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync(new StreetcodeEntity() { Id = updateDTO.StreetcodeId });

            this.historicalContextRepository
                .Setup(r => r.GetAllAsync(
                    It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<HistoricalContextEntity>,
                    IIncludableQueryable<HistoricalContextEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new List<HistoricalContextEntity>
                {
                     new () { Id = 1 },
                });

            this.timelineRepository
                  .Setup(r => r.Update(It.IsAny<TimelineItemEntity>()));

            this.SetupSaveChangesMock(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(10);
            result.Value.Title.Should().Be(updateDTO.Title);
        }

        [Fact]
        public async Task ShouldCorrectlyMapRequestToExistingEntity()
        {
            // Arrange
            var existingItem = new TimelineItemEntity
            {
                Id = 1,
                Title = "Old Title",
                Description = "Old Desc",
                HistoricalContextTimelines = new List<HistoricalContextTimeline>(),
            };

            var updateDTO = new UpdateTimelineItemDTO
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Desc",
                StreetcodeId = 1,
                HistoricalContextIds = new List<int>(),
            };
            var command = new UpdateTimelineItemCommand(updateDTO);

            this.timelineRepository
                .SetupSequence(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>,
                    IIncludableQueryable<TimelineItemEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(existingItem)
                .ReturnsAsync(existingItem);

            this.streetcodeRepository
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(), null, 
                    It.IsAny<bool>()))
                .ReturnsAsync(new StreetcodeEntity { Id = 1 });

            // Act
            await this.handler.Handle(command, CancellationToken.None);

            // Assert
            existingItem.Title.Should().Be(updateDTO.Title);
            existingItem.Description.Should().Be(updateDTO.Description);
        }
    }
}
