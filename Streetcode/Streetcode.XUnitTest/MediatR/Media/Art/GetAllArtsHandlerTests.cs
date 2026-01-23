namespace Streetcode.XUnitTest.MediatR.Media.Art
{
    using System.Linq.Expressions;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Media.Art;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Media.Art.GetAll;
    using Streetcode.DAL.Entities.Media.Images;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Xunit;

    public class GetAllArtsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly GetAllArtsHandler handler;

        public GetAllArtsHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.mapperMock = new Mock<IMapper>();
            this.loggerMock = new Mock<ILoggerService>();

            this.handler = new GetAllArtsHandler(repositoryWrapperMock.Object,
                    mapperMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenArtsExist()
        {
            // Arrange
            List<Art> arts = this.GetArtsList();
            List<ArtDTO> artDTOs = this.GetArtDTOsList();

            this.SetupMocks(arts, artDTOs);

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
            List<Art> arts = this.GetArtsList();
            List<ArtDTO> artDTOs = this.GetArtDTOsList();

            this.SetupMocks(arts, artDTOs);

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
            List<Art> arts = this.GetArtsList();
            List<ArtDTO> artDTOs = this.GetArtDTOsList();

            this.SetupMocks(arts, artDTOs);

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
            this.SetupMocks(new List<Art>(), new List<ArtDTO>());

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
            this.SetupMocks(new List<Art>(), new List<ArtDTO>());

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
            List<ArtDTO> artDTOs = this.GetArtDTOsList();

            this.SetupMocks(arts, artDTOs);

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
            List<ArtDTO> artDTOs = this.GetArtDTOsList();

            this.SetupMocks(arts, artDTOs);

            // Act
            var result = await this.handler
                .Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.Equal("Cannot find any arts", result.Errors[0].Message);
        }

        private void SetupMocks(List<Art>? arts, List<ArtDTO> artDTOs)
        {
            this.repositoryWrapperMock.Setup(r => r.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<Art, bool>>>(),
                It.IsAny<Func<IQueryable<Art>, IIncludableQueryable<Art, object>>>()))
                .ReturnsAsync(arts);

            this.mapperMock.Setup(map => map
                .Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<Art>>()))
                .Returns(artDTOs);
        }

        private List<Art> GetArtsList()
        {
            return new List<Art>
            {
                new Art { Id = 1, },
                new Art { Id = 2, },
            };
        }

        private List<ArtDTO> GetArtDTOsList()
        {
            return new List<ArtDTO>
            {
                new ArtDTO { Id = 1 },
                new ArtDTO { Id = 2 },
            };
        }
    }
}
