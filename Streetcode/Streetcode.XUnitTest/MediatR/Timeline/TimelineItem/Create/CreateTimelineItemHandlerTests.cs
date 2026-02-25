namespace Streetcode.XUnitTest.MediatR.Timeline.TimelineItem.Create
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Timeline.TimelineItem;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Timeline;
    using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
    using Streetcode.DAL.Entities.Timeline;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode;
    using Streetcode.DAL.Repositories.Interfaces.Timeline;
    using Streetcode.Resources;
    using Xunit;
    using HistoricalContextEntity = Streetcode.DAL.Entities.Timeline.HistoricalContext;
    using StreetcodeEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeContent;
    using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

    public class CreateTimelineItemHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapper;
        private readonly Mock<ILoggerService> logger;
        private readonly Mock<ITimelineRepository> timelineRepository;
        private readonly Mock<IHistoricalContextRepository> historicalContextRepository;
        private readonly Mock<IStreetcodeRepository> streetcodeRepository;
        private readonly IMapper mapper;
        private readonly CreateTimelineItemHandler handler;

        public CreateTimelineItemHandlerTests()
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

            this.handler = new CreateTimelineItemHandler(
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
        public async Task ShouldReturnFail_WhenStreetcodeNotFound()
        {
            // Arrange
            var createDTO = new CreateTimelineItemDTO
            {
                StreetcodeId = 1,
                HistoricalContextIds = new List<int> { 1 },
                Title = "Test Title",
                Description = "Test Description",
                Date = DateTime.UtcNow,
            };

            var command = new CreateTimelineItemCommand(createDTO);

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
            var expectedErrorMsg = string.Format(Messages.Error_EntityWithIdNotFound, nameof(TimelineItem), createDTO.StreetcodeId);
            result.Errors.First().Message.Should().Be(expectedErrorMsg);
        }

        [Fact]
        public async Task ShouldReturnFail_WhenSomeHistoricalContextsNotFound()
        {
            var createDTO = new CreateTimelineItemDTO
            {
                StreetcodeId = 1,
                HistoricalContextIds = new List<int> { 1 },
                Title = "Test Title",
                Description = "Test Description",
                Date = DateTime.UtcNow,
            };

            var command = new CreateTimelineItemCommand(createDTO);

            this.streetcodeRepository
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeEntity>,
                    IIncludableQueryable<StreetcodeEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync(new StreetcodeEntity { Id = createDTO.StreetcodeId });

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
        public async Task ShouldReturnOk_WhenAllDataIsValid()
        {
            var historicalContext = new HistoricalContextEntity
            {
                Id = 1,
                Title = "Test Historical Context",
            };

            var createDTO = new CreateTimelineItemDTO
            {
                StreetcodeId = 1,
                HistoricalContextIds = new List<int> { 1 },
                Title = "Test Title",
                Description = "Test Description",
                Date = DateTime.UtcNow,
            };

            var command = new CreateTimelineItemCommand(createDTO);

            this.streetcodeRepository
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeEntity>,
                    IIncludableQueryable<StreetcodeEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync(new StreetcodeEntity { Id = createDTO.StreetcodeId });

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
                .Setup(r => r.CreateAsync(It.IsAny<TimelineItemEntity>()))
                .ReturnsAsync(new TimelineItemEntity { Id = 10, Title = createDTO.Title });

            this.SetupSaveChangesMock(1);

            this.timelineRepository
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>,
                    IIncludableQueryable<TimelineItemEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new TimelineItemEntity
                {
                    Id = 10,
                    Title = createDTO.Title,
                    HistoricalContextTimelines = new List<HistoricalContextTimeline>
                    {
                        new () {
                        HistoricalContextId = 1,
                        HistoricalContext = historicalContext,
                        },
                    },
                });

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(10);
            result.Value.Title.Should().Be(createDTO.Title);
        }

        [Fact]
        public async Task ShouldReturnOk_WhenHistoricalContextIdsIsEmpty()
        {
            var createDTO = new CreateTimelineItemDTO
            {
                StreetcodeId = 1,
                HistoricalContextIds = new List<int>(),
                Title = "Test Title",
                Description = "Test Description",
                Date = DateTime.UtcNow,
            };

            var command = new CreateTimelineItemCommand(createDTO);

            this.streetcodeRepository
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeEntity>,
                    IIncludableQueryable<StreetcodeEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync(new StreetcodeEntity { Id = createDTO.StreetcodeId });

            this.timelineRepository
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>,
                    IIncludableQueryable<TimelineItemEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new TimelineItemEntity
                {
                    Id = 10,
                    Title = createDTO.Title,
                    HistoricalContextTimelines = new List<HistoricalContextTimeline>(),
                });

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(10);
            result.Value.Title.Should().Be(createDTO.Title);
        }
    }
}
