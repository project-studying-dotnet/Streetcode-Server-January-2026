namespace Streetcode.XUnitTest.MediatR.Timeline.TimelineItem.Delete
{
    using System.Linq.Expressions;
    using FluentAssertions;
    using global::MediatR;
    using Moq;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.Resources;
    using Xunit;
    using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

    public class DeleteTimelineItemHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly DeleteTimelineItemHandler handler;

        public DeleteTimelineItemHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.loggerMock = new Mock<ILoggerService>();
            this.handler = new DeleteTimelineItemHandler(
                this.loggerMock.Object,
                this.repositoryWrapperMock.Object);
        }

        [Fact]
        public async Task Handle_TimelineItemNotFound_ReturnsFailResult()
        {
            // Arrange
            int testId = 1;
            var command = new DeleteTimelineItemCommand(testId);

            this.repositoryWrapperMock
                .Setup(r => r.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync((TimelineItemEntity?)null);

            var expectedErrorMsg = string.Format(Messages.Error_EntityWithIdNotFound, nameof(TimelineItem), testId);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedErrorMsg);

            this.loggerMock.Verify(x => x.LogError(command, expectedErrorMsg), Times.Once);
            this.repositoryWrapperMock.Verify(r => r.TimelineRepository.Delete(It.IsAny<TimelineItemEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_TimelineItemExists_DeletesAndReturnsOk()
        {
            // Arrange
            int testId = 1;
            var command = new DeleteTimelineItemCommand(testId);
            var existingTimelineItem = new TimelineItemEntity { Id = testId };

            this.repositoryWrapperMock
                .Setup(r => r.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(existingTimelineItem);

            this.repositoryWrapperMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(Unit.Value);
        }
    }
}
