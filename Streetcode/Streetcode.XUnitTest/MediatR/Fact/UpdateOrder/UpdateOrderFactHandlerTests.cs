namespace Streetcode.XUnitTest.MediatR.Fact.UpdateOrder
{
    using System.Linq.Expressions;
    using AutoMapper;
    using global::MediatR;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.TextContent;
    using Streetcode.BLL.MediatR.Streetcode.Fact.UpdateOrder;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
    using Xunit;
    using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

    public class UpdateOrderFactHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IFactRepository> factRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly UpdateOrderFactHandler handler;

        public UpdateOrderFactHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.factRepositoryMock = new Mock<IFactRepository>();
            this.loggerMock = new Mock<ILoggerService>();

            this.repositoryWrapperMock.Setup(r => r.FactRepository).Returns(this.factRepositoryMock.Object);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new FactProfile());
            });
            this.mapper = new Mapper(configuration);

            this.handler = new UpdateOrderFactHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        private void SetupRepositoryData(List<FactEntity> fakeDbData)
        {
            this.factRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<FactEntity, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync((
                    Expression<Func<FactEntity, bool>> predicate,
                    Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>> include,
                    bool trackChanges) =>
                {
                    return fakeDbData.FirstOrDefault(predicate.Compile());
                });
        }

        private void SetupSaveChanges(int result)
        {
            this.repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(result);
        }

        private static List<UpdateFactOrderDTO> GetUpdateOrderDTOs()
        {
            return
            [
                new UpdateFactOrderDTO { Id = 1, Order = 5 },
                new UpdateFactOrderDTO { Id = 2, Order = 10 },
            ];
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenFactsExist_AndOrderIsUpdated()
        {
            // Arrange
            var dtos = GetUpdateOrderDTOs();
            var command = new UpdateOrderFactCommand(dtos);

            var fakeDbData = new List<FactEntity>
            {
                new () { Id = 1, Order = 1 },
                new () { Id = 2, Order = 2 },
            };

            this.SetupRepositoryData(fakeDbData);
            this.SetupSaveChanges(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Unit.Value, result.Value);

            this.factRepositoryMock.Verify(x => x.Update(It.Is<FactEntity>(f => f.Id == 1 && f.Order == 5)), Times.Once);
            this.factRepositoryMock.Verify(x => x.Update(It.Is<FactEntity>(f => f.Id == 2 && f.Order == 10)), Times.Once);

            this.repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessButLogsWarning_WhenSomeFactsAreMissing()
        {
            // Arrange
            var dtos = new List<UpdateFactOrderDTO>
            {
                new () { Id = 1, Order = 5 },
                new () { Id = 99, Order = 10 },
            };
            var command = new UpdateOrderFactCommand(dtos);

            var fakeDbData = new List<FactEntity>
            {
                new () { Id = 1, Order = 1 },
            };

            this.SetupRepositoryData(fakeDbData);
            this.SetupSaveChanges(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            this.factRepositoryMock.Verify(x => x.Update(It.Is<FactEntity>(f => f.Id == 1)), Times.Once);
            this.factRepositoryMock.Verify(x => x.Update(It.Is<FactEntity>(f => f.Id == 99)), Times.Never);

            this.loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("Fact with TermId 99 not found"))),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenSaveChangesFails()
        {
            // Arrange
            var dtos = GetUpdateOrderDTOs();
            var command = new UpdateOrderFactCommand(dtos);
            var expectedError = "Error while updating facts order";

            var fakeDbData = new List<FactEntity>
            {
                new () { Id = 1 },
                new () { Id = 2 },
            };

            this.SetupRepositoryData(fakeDbData);
            this.SetupSaveChanges(0);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(expectedError, result.Errors.First().Message);

            this.loggerMock.Verify(x => x.LogError(It.IsAny<object>(), expectedError), Times.Once);
        }
    }
}