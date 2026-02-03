namespace Streetcode.XUnitTest.MediatR.Fact.GetByStreetcodeId
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.TextContent;
    using Streetcode.BLL.MediatR.Streetcode.Fact.GetByStreetcodeId;
    using Streetcode.DAL.Entities.Media.Images;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
    using Xunit;
    using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

    public class GetFactByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IFactRepository> factRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly GetFactByStreetcodeIdHandler handler;

        public GetFactByStreetcodeIdHandlerTests()
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

            this.handler = new GetFactByStreetcodeIdHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        private void SetupGetByStreetcodeId(List<FactEntity>? facts)
        {
            this.factRepositoryMock
                .Setup(r => r.GetAllAsync(
                    It.IsAny<Expression<Func<FactEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()))
                .ReturnsAsync(facts);
        }

        private static List<FactEntity> GetFactsList(int streetcodeId)
        {
            return new List<FactEntity>
            {
                new FactEntity
                {
                    Id = 1,
                    Title = "Fact 1",
                    FactContent = "Content 1",
                    StreetcodeId = streetcodeId,
                    ImageId = 1,
                    Order = 1,
                    Image = new Image { Id = 1, ImageDetails = new ImageDetails { Title = "Img1" } },
                },
                new FactEntity
                {
                    Id = 2,
                    Title = "Fact 2",
                    FactContent = "Content 2",
                    StreetcodeId = streetcodeId,
                    ImageId = 2,
                    Order = 2,
                    Image = new Image { Id = 2, ImageDetails = new ImageDetails { Title = "Img2" } },
                },
            };
        }

        private static List<FactDTO> GetFactDTOsList(int streetcodeId)
        {
            return new List<FactDTO>
            {
                new FactDTO
                {
                    Id = 1,
                    Title = "Fact 1",
                    FactContent = "Content 1",
                    ImageId = 1,
                    Order = 1,
                    ImageDescription = "Img1",
                },
                new FactDTO
                {
                    Id = 2,
                    Title = "Fact 2",
                    FactContent = "Content 2",
                    ImageId = 2,
                    Order = 2,
                    ImageDescription = "Img2",
                },
            };
        }

        [Fact]
        public async Task Handle_ReturnsSuccessResult_WhenFactsExist()
        {
            // Arrange
            int streetcodeId = 1;
            var facts = GetFactsList(streetcodeId);
            var expectedDtos = GetFactDTOsList(streetcodeId);

            this.SetupGetByStreetcodeId(facts);

            // Act
            var result = await this.handler.Handle(new GetFactByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(facts.Count, result.Value.Count());
            result.Value.Should().BeEquivalentTo(expectedDtos);
        }

        [Fact]
        public async Task Handle_ReturnsSortedByOrderDescending_WhenFactsExist()
        {
            // Arrange
            int streetcodeId = 1;
            var unsortedFacts = new List<FactEntity>
            {
                new FactEntity { Id = 1, StreetcodeId = streetcodeId, Order = 1, Image = new Image() },
                new FactEntity { Id = 2, StreetcodeId = streetcodeId, Order = 5, Image = new Image() },
                new FactEntity { Id = 3, StreetcodeId = streetcodeId, Order = 2, Image = new Image() },
            };

            this.SetupGetByStreetcodeId(unsortedFacts);

            // Act
            var result = await this.handler.Handle(new GetFactByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            result.Value.Should().BeInDescendingOrder(x => x.Order);
            result.Value.First().Order.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ReturnsEmpty_WhenFactsDoNotExist()
        {
            // Arrange
            int streetcodeId = 1;
            this.SetupGetByStreetcodeId(new List<FactEntity>());

            // Act
            var result = await this.handler.Handle(new GetFactByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);

            this.loggerMock.Verify(
                x => x.LogError(It.IsAny<object>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}