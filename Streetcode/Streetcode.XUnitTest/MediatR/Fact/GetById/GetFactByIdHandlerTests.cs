namespace Streetcode.XUnitTest.MediatR.Fact.GetById
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.TextContent;
    using Streetcode.BLL.MediatR.Streetcode.Fact.GetById;
    using Streetcode.DAL.Entities.Media.Images;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
    using Xunit;
    using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

    public class GetFactByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IFactRepository> factRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly GetFactByIdHandler handler;

        public GetFactByIdHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.factRepositoryMock = new Mock<IFactRepository>();
            this.loggerMock = new Mock<ILoggerService>();

            this.repositoryWrapperMock
                .Setup(r => r.FactRepository)
                .Returns(this.factRepositoryMock.Object);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new FactProfile());
            });
            this.mapper = new Mapper(configuration);

            this.handler = new GetFactByIdHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        private void SetupGetFactById(FactEntity? fact)
        {
            this.factRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<FactEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()))
                .ReturnsAsync(fact);
        }

        private static FactEntity GetFactEntity(int id)
        {
            return new FactEntity
            {
                Id = id,
                Title = "Test Fact Title",
                FactContent = "Test Fact Content",
                ImageId = 10,
                StreetcodeId = 1,
                Order = 1,
                Image = new Image
                {
                    Id = 10,
                    ImageDetails = new ImageDetails
                    {
                        Id = 5,
                        Title = "Test Image Description",
                    },
                },
            };
        }

        private static FactDTO GetFactDTO(int id)
        {
            return new FactDTO
            {
                Id = id,
                Title = "Test Fact Title",
                FactContent = "Test Fact Content",
                ImageId = 10,
                Order = 1,
                ImageDescription = "Test Image Description",
            };
        }

        [Fact]
        public async Task Handle_ReturnsSuccessResult_WhenFactExists()
        {
            // Arrange
            var testId = 1;
            var factEntity = GetFactEntity(testId);
            var expectedFactDto = GetFactDTO(testId);

            this.SetupGetFactById(factEntity);

            // Act
            var result = await this.handler.Handle(new GetFactByIdQuery(testId), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            result.Value.Should().BeEquivalentTo(expectedFactDto);
        }

        [Fact]
        public async Task Handle_ReturnsFailedResult_WhenFactDoesNotExist()
        {
            // Arrange
            var testId = 1;
            string expectedErrorMsg = $"Cannot find any fact with corresponding id: {testId}";

            this.SetupGetFactById(null);

            // Act
            var result = await this.handler.Handle(new GetFactByIdQuery(testId), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Single(result.Errors);
            Assert.Equal(expectedErrorMsg, result.Errors.First().Message);

            this.loggerMock.Verify(
                x => x.LogError(It.IsAny<object>(), expectedErrorMsg),
                Times.Once);
        }
    }
}