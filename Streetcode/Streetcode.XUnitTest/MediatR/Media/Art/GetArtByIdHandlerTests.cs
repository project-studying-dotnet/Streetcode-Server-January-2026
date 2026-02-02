namespace Streetcode.XUnitTest.MediatR.Media.Art
{
    using System.Linq.Expressions;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Media.Art;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Media.Art.GetById;
    using Streetcode.DAL.Entities.Media.Images;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Xunit;

    public class GetArtByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly GetArtByIdHandler handler;

        public GetArtByIdHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.mapperMock = new Mock<IMapper>();
            this.loggerMock = new Mock<ILoggerService>();

            this.handler = new GetArtByIdHandler(repositoryWrapperMock.Object,
                    mapperMock.Object, loggerMock.Object);
        }

        [Theory]
        [InlineData(1)]
        public async Task Handle_ReturnsSuccess_WhenArtExist(int artId)
        {
            // Arrange
            Art art = this.GetArt();
            ArtDTO artDTO = this.GetArtDTO();

            this.SetupMocks(art, artDTO);

            // Act
            var result = await this.handler
                .Handle(new GetArtByIdQuery(artId), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(1)]
        public async Task Handle_ReturnsCorrectArt_WhenArtExist(int artId)
        {
            // Arrange
            Art art = this.GetArt();
            ArtDTO artDTO = this.GetArtDTO();

            this.SetupMocks(art, artDTO);

            // Act
            var result = await this.handler
                .Handle(new GetArtByIdQuery(artId), CancellationToken.None);

            // Assert
            Assert.Equal(artId, result.Value.Id);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Handle_ReturnsFailStatus_WhenArtsAreNull(int artId)
        {
            // Arrange
            Art? art = null;
            ArtDTO artDTO = this.GetArtDTO();

            this.SetupMocks(art, artDTO);

            // Act
            var result = await this.handler
                .Handle(new GetArtByIdQuery(artId), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Handle_ReturnsErrorMessage_WhenArtsAreNull(int artId)
        {
            // Arrange
            Art? art = null;
            ArtDTO artDTO = this.GetArtDTO();

            this.SetupMocks(art, artDTO);

            // Act
            var result = await this.handler
                .Handle(new GetArtByIdQuery(artId), CancellationToken.None);

            // Assert
            Assert.Equal(
                $"Cannot find an art with corresponding id: {artId}",
                result.Errors[0].Message);
        }

        private void SetupMocks(Art? art, ArtDTO artDTO)
        {
            this.repositoryWrapperMock.Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Art, bool>>>(),
                It.IsAny<Func<IQueryable<Art>, IIncludableQueryable<Art, object>>>(),
                It.IsAny<bool>()))
                .ReturnsAsync(art);

            this.mapperMock.Setup(map => map
                .Map<ArtDTO>(It.IsAny<Art>()))
                .Returns(artDTO);
        }

        private Art GetArt()
        {
            return new Art { Id = 1 };
        }

        private ArtDTO GetArtDTO()
        {
            return new ArtDTO { Id = 1 };
        }
    }
}
