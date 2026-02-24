namespace Streetcode.XUnitTest.MediatR.Timeline.HistoricalContext.Delete
{
    using System.Linq.Expressions;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Timeline.HistoricalContext.Delete;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Timeline;
    using Streetcode.Resources;
    using Streetcode.Shared.Extensions;
    using Xunit;
    using HistoricalContextEntity = Streetcode.DAL.Entities.Timeline.HistoricalContext;

    public class DeleteHistoricalContextHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> mockRepositoryWrapper;
        private readonly Mock<ILoggerService> mockLoggerService;
        private readonly Mock<IHistoricalContextRepository> mockHistoricalContextRepository;
        private readonly DeleteHistoricalContextHandler handler;

        public DeleteHistoricalContextHandlerTests()
        {
            this.mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            this.mockLoggerService = new Mock<ILoggerService>();
            this.mockHistoricalContextRepository = new Mock<IHistoricalContextRepository>();

            this.mockRepositoryWrapper
                .Setup(r => r.HistoricalContextRepository)
                .Returns(this.mockHistoricalContextRepository.Object);

            this.handler = new DeleteHistoricalContextHandler(
                this.mockLoggerService.Object,
                this.mockRepositoryWrapper.Object);
        }

        private void SetupSaveChangesMock(int result)
        {
            this.mockRepositoryWrapper
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(result);
        }

        [Fact]
        public async Task Handler_ReturnOk_WhenContentExists()
        {
            var existingEntity = new HistoricalContextEntity
            {
                Id = 1,
                Title = "Test",
            };

            var command = new DeleteHistoricalContextCommand(1);

            this.mockHistoricalContextRepository
                .Setup(h => h.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HistoricalContextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<HistoricalContextEntity>,
                    IIncludableQueryable<HistoricalContextEntity, object>>>(),
                    It.IsAny<bool>()))
            .ReturnsAsync(existingEntity);

            this.mockHistoricalContextRepository
                .Setup(h => h.Delete(
                    It.IsAny<HistoricalContextEntity>()));

            this.mockRepositoryWrapper
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenContextNotExist()
        {
            var command = new DeleteHistoricalContextCommand(1);

            this.mockHistoricalContextRepository
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
            result.Errors.Should().ContainSingle(e =>
                e.Message.Contains(command.Id.ToString()) &&
                e.Message.Contains("HistoricalContext"));
        }
    }
}
