namespace Streetcode.XUnitTest.MediatR.Media.Art
{
    using System.Linq.Expressions;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Repositories.Interfaces;
    using Streetcode.BLL.DTO.Media.Art;
    using Streetcode.BLL.DTO.Media.Images;
    using Streetcode.BLL.Interfaces.BlobStorage;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Media.Images;
    using Streetcode.BLL.MediatR.Media.Art.GetByStreetcodeId;
    using Streetcode.DAL.Entities.Media.Images;
    using Streetcode.DAL.Entities.Streetcode;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Xunit;

    public class GetArtsByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly IMapper mapper;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly Mock<IBlobService> blobServiceMock;
        private readonly GetArtsByStreetcodeIdHandler handler;

        public GetArtsByStreetcodeIdHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.loggerMock = new Mock<ILoggerService>();
            this.blobServiceMock = new Mock<IBlobService>();

            this.repositoryWrapperMock.Setup(r => r.ArtRepository)
                .Returns(new Mock<IArtRepository>().Object);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ArtProfile());
                cfg.AddProfile(new ImageProfile());
            });
            this.mapper = new Mapper(configuration);

            this.handler = new GetArtsByStreetcodeIdHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.blobServiceMock.Object,
                this.loggerMock.Object);
        }

        [Theory]
        [InlineData(1)]
        public async Task Handle_ReturnsSuccess_WhenArtsExist(int streetcodeId)
        {
            // Arrange
            List<Art> arts = GetArtsList();

            this.SetupArts(arts);
            this.SetupBlobService("test_base64_string");

            // Act
            var result = await this.handler
                .Handle(new GetArtsByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(1)]
        public async Task Handle_ReturnsCorrectType_WhenArtsExist(int streetcodeId)
        {
            // Arrange
            List<Art> arts = GetArtsList();

            this.SetupArts(arts);
            this.SetupBlobService("test_base64_string");

            // Act
            var result = await this.handler
                .Handle(new GetArtsByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            Assert.IsType<IEnumerable<ArtDTO>>(result.Value, exactMatch: false);
        }

        [Theory]
        [InlineData(1)]
        public async Task Handle_ReturnsCorrectAmount_WhenArtsExist(int streetcodeId)
        {
            // Arrange
            List<Art> arts = GetArtsList();
            List<ArtDTO> artDTOs = GetArtDTOsList();

            this.SetupArts(arts);
            this.SetupBlobService("test_base64_string");

            // Act
            var result = await this.handler
                .Handle(new GetArtsByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            Assert.Equal(artDTOs.Count, result.Value.Count());
        }

        [Theory]
        [InlineData(1, "test_base64_string")]
        public async Task Handle_ReturnsCorrectData_WhenArtsExist(int streetcodeId, string base64)
        {
            // Arrange
            List<Art> arts = GetArtsList();

            this.SetupArts(arts);
            this.SetupBlobService(base64);

            // Act
            var result = await this.handler
                .Handle(new GetArtsByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            Assert.Equal(base64, result.Value?.FirstOrDefault()?.Image?.Base64);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Handle_ReturnsSuccessAndEmpty_WhenArtsAreNullOrEmpty(int streetcodeId)
        {
            // Arrange
            this.SetupArts(new List<Art>());

            // Act
            var result = await this.handler
                .Handle(new GetArtsByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        private void SetupArts(List<Art>? arts)
        {
            var artRepository = new Mock<IArtRepository>();

            artRepository.Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<Art, bool>>>(),
                It.IsAny<Func<IQueryable<Art>, IIncludableQueryable<Art, object>>>()))
                .ReturnsAsync(arts);

            this.repositoryWrapperMock.Setup(r => r.ArtRepository).Returns(artRepository.Object);
        }

        private void SetupBlobService(string base64String)
        {
            this.blobServiceMock.Setup(b => b.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns(base64String);
        }

        private static List<Art> GetArtsList()
        {
            return
            [
                new Art
                {
                    Id = 1,
                    Image = new Image { Id = 1, BlobName = "test1.png" },
                    StreetcodeArts =
                    [
                        new StreetcodeArt { StreetcodeId = 1 },
                    ],
                },
                new Art
                {
                    Id = 2,
                    Image = new Image { Id = 2, BlobName = "test2.png" },
                    StreetcodeArts =
                    [
                        new StreetcodeArt { StreetcodeId = 1 },
                    ],
                },
            ];
        }

        private static List<ArtDTO> GetArtDTOsList()
        {
            return
            [
                new ArtDTO
                {
                    Id = 1,
                    ImageId = 1,
                    Image = new ImageDTO { Id = 1, BlobName = "test1.png" },
                },
                new ArtDTO
                {
                    Id = 2,
                    ImageId = 2,
                    Image = new ImageDTO { Id = 2, BlobName = "test2.png" },
                },
            ];
        }
    }
}
