namespace Streetcode.XUnitTest.MediatR.Media.Art
{
    using System.Linq.Expressions;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Media.Art;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Media.Images;
    using Streetcode.BLL.MediatR.Media.Art.GetAll;
    using Streetcode.DAL.Entities.Media.Images;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Xunit;

    public class GetAllArtsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly IMapper mapper;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly GetAllArtsHandler handler;

        public GetAllArtsHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.loggerMock = new Mock<ILoggerService>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ArtProfile());
            });
            this.mapper = new Mapper(configuration);

            this.handler = new GetAllArtsHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenArtsExist()
        {
            // Arrange
            List<Art> arts = GetArtsList();

            this.SetupMocks(arts);

            // Act
            var result = await this.handler
                .Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectNumberOfArts_WhenArtsExist()
        {
            // Arrange
            List<Art> arts = GetArtsList();

            this.SetupMocks(arts);

            // Act
            var result = await this.handler
                .Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(arts.Count, result.Value.Count());
        }

        [Fact]
        public async Task Handle_ReturnsCorrectType_WhenArtsExist()
        {
            // Arrange
            List<Art> arts = GetArtsList();

            this.SetupMocks(arts);

            // Act
            var result = await this.handler
                .Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<ArtDTO>>(result.Value);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenArtsAreEmpty()
        {
            // Arrange
            this.SetupMocks([]);

            // Act
            var result = await this.handler
                .Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ReturnsEmpty_WhenArtsAreEmpty()
        {
            // Arrange
            this.SetupMocks([]);

            // Act
            var result = await this.handler
                .Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task Handle_ReturnsFailStatus_WhenArtsAreNull()
        {
            // Arrange
            List<Art>? arts = null;

            this.SetupMocks(arts);

            // Act
            var result = await this.handler
                .Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task Handle_ReturnsErrorMessage_WhenArtsAreNull()
        {
            // Arrange
            List<Art>? arts = null;

            this.SetupMocks(arts);

            // Act
            var result = await this.handler
                .Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.Equal("Cannot find any arts", result.Errors[0].Message);
        }

        private void SetupMocks(List<Art>? arts)
        {
            this.repositoryWrapperMock.Setup(r => r.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<Art, bool>>>(),
                It.IsAny<Func<IQueryable<Art>, IIncludableQueryable<Art, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(arts);
        }

        private static List<Art> GetArtsList()
        {
            return
            [
                new Art { Id = 1, },
                new Art { Id = 2, },
            ];
        }
    }
}
