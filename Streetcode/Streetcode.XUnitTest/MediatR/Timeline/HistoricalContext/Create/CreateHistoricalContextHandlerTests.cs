namespace Streetcode.XUnitTest.MediatR.Timeline.HistoricalContext.Create
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Timeline.HistoricalContext;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Timeline;
    using Streetcode.BLL.MediatR.Timeline.HistoricalContext.Create;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Timeline;
    using Streetcode.Resources;
    using Xunit;
    using HistoricalContextEntity = Streetcode.DAL.Entities.Timeline.HistoricalContext;

    public class CreateHistoricalContextHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> mockRepositoryWrapper;
        private readonly Mock<ILoggerService> mockLoggerService;
        private readonly Mock<IHistoricalContextRepository> mockHistoricalContextRepository;
        private readonly IMapper mapper;
        private readonly CreateHistoricalContextHandler handler;

        public CreateHistoricalContextHandlerTests()
        {
            this.mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            this.mockLoggerService = new Mock<ILoggerService>();
            this.mockHistoricalContextRepository = new Mock<IHistoricalContextRepository>();

            this.mockRepositoryWrapper
                .Setup(r => r.HistoricalContextRepository)
                .Returns(this.mockHistoricalContextRepository.Object);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new HistoricalContextProfile());
            });

            this.mapper = new Mapper(configuration);

            this.handler = new CreateHistoricalContextHandler(
                this.mockRepositoryWrapper.Object,
                this.mockLoggerService.Object,
                this.mapper);
        }

        private void SetupSaveChangesMock(int result)
        {
            this.mockRepositoryWrapper
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(result);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenCreateIsSuccessful()
        {
            // Arrange
            var createDTO = new CreateHistoricalContextDTO
            {
                Title = "Test Title",
            };

            var createEntity = new HistoricalContextEntity
            {
                Id = 1,
                Title = createDTO.Title,
            };

            var createdDTO = new HistoricalContextDTO
            {
                Id = createEntity.Id,
                Title = createEntity.Title,
            };

            var command = new CreateHistoricalContextCommand(createDTO);

            this.mockHistoricalContextRepository
                .Setup(h => h.CreateAsync(It.IsAny<HistoricalContextEntity>()))
                .ReturnsAsync(createEntity);

            this.mockHistoricalContextRepository
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<HistoricalContextEntity>,
                    IIncludableQueryable<HistoricalContextEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync((HistoricalContextEntity)null!);

            this.SetupSaveChangesMock(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(createdDTO);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_IfTitleExists()
        {
            // Arrange
            var createDTO = new CreateHistoricalContextDTO
            {
                Title = "Existing Title",
            };

            var existingEntity = new HistoricalContextEntity
            {
                Id = 1,
                Title = createDTO.Title,
            };

            var command = new CreateHistoricalContextCommand(createDTO);

            this.mockHistoricalContextRepository
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<HistoricalContextEntity>,
                    IIncludableQueryable<HistoricalContextEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync(existingEntity);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().ContainSingle(e => e.Message.Contains(Messages.Error_HistoricalContextTitleAlreadyExists));
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_IfSaveUnsuccessful()
        {
            // Arrange
            var createDTO = new CreateHistoricalContextDTO
            {
                Title = "Test Title",
            };
            var createEntity = new HistoricalContextEntity
            {
                Id = 1,
                Title = createDTO.Title,
            };

            var command = new CreateHistoricalContextCommand(createDTO);

            this.mockHistoricalContextRepository
                .Setup(h => h.CreateAsync(It.IsAny<HistoricalContextEntity>()))
                .ReturnsAsync(createEntity);

            this.mockHistoricalContextRepository
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<HistoricalContextEntity>,
                    IIncludableQueryable<HistoricalContextEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync(createEntity);

            this.SetupSaveChangesMock(0);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
        }
    }
}
