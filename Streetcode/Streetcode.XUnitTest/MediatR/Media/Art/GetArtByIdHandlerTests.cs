namespace Streetcode.XUnitTest.MediatR.Media.Art
{
    using System.Linq.Expressions;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Media.Art;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Media.Images;
    using Streetcode.BLL.MediatR.Media.Art.GetById;
    using Streetcode.DAL.Entities.Media.Images;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Xunit;

    public class GetArtByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly IMapper mapper;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly GetArtByIdHandler handler;

        public GetArtByIdHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.loggerMock = new Mock<ILoggerService>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ArtProfile());
            });
            this.mapper = new Mapper(configuration);

            this.handler = new GetArtByIdHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        [Theory]
        [InlineData(1)]
        public async Task Handle_ReturnsSuccess_WhenArtExist(int artId)
        {
            // Arrange
            Art art = GetArt();

            this.SetupMocks(art);

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
            Art art = GetArt();

            this.SetupMocks(art);

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

            this.SetupMocks(art);

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

            this.SetupMocks(art);

            // Act
            var result = await this.handler
                .Handle(new GetArtByIdQuery(artId), CancellationToken.None);

            // Assert
            Assert.Equal(
                $"Cannot find an art with corresponding id: {artId}",
                result.Errors[0].Message);
        }

        private void SetupMocks(Art? art)
        {
            this.repositoryWrapperMock.Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Art, bool>>>(),
                It.IsAny<Func<IQueryable<Art>, IIncludableQueryable<Art, object>>>(),
                It.IsAny<bool>()))
                .ReturnsAsync(art);
        }

        private static Art GetArt()
        {
            return new Art { Id = 1 };
        }
    }
}
