namespace Streetcode.XUnitTest.MediatR.Timeline.HistoricalContext.Update
{
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Timeline.HistoricalContext;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Timeline;
    using Streetcode.BLL.MediatR.Timeline.HistoricalContext.Update;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Timeline;
    using Streetcode.Resources;
    using System.Linq.Expressions;
    using Xunit;
    using HistoricalContextEntity = Streetcode.DAL.Entities.Timeline.HistoricalContext;

    public class UpdateHistoricalContextHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly Mock<IHistoricalContextRepository> historicalContextRepositoryMock;
        private readonly IMapper mapper;
        private readonly UpdateHistoricalContextHandler handler;

        public UpdateHistoricalContextHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.loggerMock = new Mock<ILoggerService>();
            this.historicalContextRepositoryMock = new Mock<IHistoricalContextRepository>();

            this.repositoryWrapperMock
                .Setup(r => r.HistoricalContextRepository)
                .Returns(this.historicalContextRepositoryMock.Object);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new HistoricalContextProfile());
            });

            this.mapper = new Mapper(configuration);

            this.handler = new UpdateHistoricalContextHandler(
                this.repositoryWrapperMock.Object,
                this.loggerMock.Object,
                this.mapper);
        }

        private void SetupSaveChangesMock(int result)
        {
            this.repositoryWrapperMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(result);
        }

        [Fact]
        public async Task Handler_ShouldReturnOk_IfUpdateSuccesful()
        {
            // Arrange
            var updateDTO = new UpdateHistoricalContextDTO
            {
                Id = 1,
                Title = "Updated Title",
            };

            var existingEntity = new HistoricalContextEntity
            {
                Id = 1,
                Title = "Old Title",
            };

            var updatedEntity = new HistoricalContextEntity
            {
                Id = 1,
                Title = updateDTO.Title,
            };

            var command = new UpdateHistoricalContextCommand(updateDTO);

            this.historicalContextRepositoryMock
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<HistoricalContextEntity>,
                    IIncludableQueryable<HistoricalContextEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync(updatedEntity);

            this.historicalContextRepositoryMock
                .Setup(h => h.Update(
                It.IsAny<HistoricalContextEntity>()))
                .Returns((EntityEntry<HistoricalContextEntity>)null!);

            this.SetupSaveChangesMock(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(updateDTO.Id);
            result.Value.Title.Should().Be(updateDTO.Title);
        }

        [Fact]
        public async Task Handle_ReturnsFail_IfHistoricalContextDoesNotExist()
        {
            // Arrange

            var updateDTO = new UpdateHistoricalContextDTO
            {
                Id = 1,
                Title = "Updated Title",
            };

            var command = new UpdateHistoricalContextCommand(updateDTO);

            this.historicalContextRepositoryMock
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<HistoricalContextEntity>,
                    IIncludableQueryable<HistoricalContextEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((HistoricalContextEntity)null!);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message.Contains(updateDTO.Id.ToString()));
        }

        [Fact]
        public async Task Handle_ReturnFail_IfSaveUnsuccessful()
        {
            // Arrange
            var updateDTO = new UpdateHistoricalContextDTO
            {
                Id = 1,
                Title = "Update title",
            };

            var exitingDTO = new HistoricalContextEntity
            {
                Id = 1,
                Title = "Title",
            };

            var updatedDTO = new HistoricalContextDTO
            {
                Id = 1,
                Title = updateDTO.Title,
            };

            var command = new UpdateHistoricalContextCommand(updateDTO);

            this.historicalContextRepositoryMock
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<HistoricalContextEntity>,
                    IIncludableQueryable<HistoricalContextEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync(exitingDTO);

            this.historicalContextRepositoryMock
                .Setup(h => h.Update(
                It.IsAny<HistoricalContextEntity>()))
                .Returns((EntityEntry<HistoricalContextEntity>)null!);

            this.SetupSaveChangesMock(0);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
        }
    }
}
