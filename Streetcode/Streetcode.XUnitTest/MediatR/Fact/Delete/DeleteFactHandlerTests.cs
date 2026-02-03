namespace Streetcode.XUnitTest.MediatR.Fact.Delete
{
    using System.Linq.Expressions;
    using global::MediatR;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Streetcode.Fact.Delete;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
    using Xunit;
    using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

    public class DeleteFactHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IFactRepository> factRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly DeleteFactHandler handler;

        public DeleteFactHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.factRepositoryMock = new Mock<IFactRepository>();
            this.loggerMock = new Mock<ILoggerService>();

            this.repositoryWrapperMock
                .Setup(r => r.FactRepository)
                .Returns(this.factRepositoryMock.Object);

            this.handler = new DeleteFactHandler(
                this.repositoryWrapperMock.Object,
                this.loggerMock.Object);
        }

        private void SetupGetById(FactEntity? fact)
        {
            this.factRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<FactEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()))
                .ReturnsAsync(fact);
        }

        private void SetupSaveChanges(int result)
        {
            this.repositoryWrapperMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(result);
        }

        private static FactEntity GetFactEntity(int id)
        {
            return new FactEntity
            {
                Id = id,
                Title = "Test Fact",
                FactContent = "Content",
            };
        }

        [Fact]
        public async Task Handle_ReturnsSuccessResult_WhenFactExistsAndDeleted()
        {
            // Arrange
            int testId = 1;
            var factEntity = GetFactEntity(testId);

            this.SetupGetById(factEntity);
            this.SetupSaveChanges(1);

            // Act
            var result = await this.handler.Handle(new DeleteFactCommand(testId), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Unit.Value, result.Value);

            this.factRepositoryMock.Verify(x => x.Delete(It.Is<FactEntity>(f => f.Id == testId)), Times.Once);
            this.repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailedResult_WhenFactDoesNotExist()
        {
            // Arrange
            int testId = 1;
            string expectedErrorMsg = $"Cannot find a fact with Id: {testId}";

            this.SetupGetById(null);

            // Act
            var result = await this.handler.Handle(new DeleteFactCommand(testId), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(expectedErrorMsg, result.Errors.First().Message);

            this.factRepositoryMock.Verify(x => x.Delete(It.IsAny<FactEntity>()), Times.Never);
            this.loggerMock.Verify(x => x.LogError(It.IsAny<object>(), expectedErrorMsg), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailedResult_WhenSaveChangesFails()
        {
            // Arrange
            int testId = 1;
            var factEntity = GetFactEntity(testId);
            string expectedErrorMsg = "Error while saving changes to database";

            this.SetupGetById(factEntity);
            this.SetupSaveChanges(0);

            // Act
            var result = await this.handler.Handle(new DeleteFactCommand(testId), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(expectedErrorMsg, result.Errors.First().Message);

            this.factRepositoryMock.Verify(x => x.Delete(It.IsAny<FactEntity>()), Times.Once);
            this.loggerMock.Verify(x => x.LogError(It.IsAny<object>(), expectedErrorMsg), Times.Once);
        }
    }
}