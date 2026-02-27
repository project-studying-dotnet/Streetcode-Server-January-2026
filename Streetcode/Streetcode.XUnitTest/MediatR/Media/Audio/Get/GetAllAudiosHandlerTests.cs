using Streetcode.Shared.Extensions;

namespace Streetcode.XUnitTest.MediatR.Media.Audio.Get
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Media.Audio;
    using Streetcode.BLL.Interfaces.BlobStorage;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Media;
    using Streetcode.BLL.MediatR.Media.Audio.GetAll;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.Resources;
    using Xunit;
    using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

    public class GetAllAudiosHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> mockRepositoryWrapper;
        private readonly Mock<IBlobService> mockBlobService;
        private readonly Mock<ILoggerService> mockLogger;
        private readonly IMapper mapper;
        private readonly GetAllAudiosHandler handler;

        public GetAllAudiosHandlerTests()
        {
            this.mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            this.mockBlobService = new Mock<IBlobService>();
            this.mockLogger = new Mock<ILoggerService>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AudioProfile>();
            });

            this.mapper = new Mapper(configuration);

            this.handler = new GetAllAudiosHandler(
                this.mockRepositoryWrapper.Object,
                this.mapper,
                this.mockBlobService.Object,
                this.mockLogger.Object);
        }

        [Fact]
        public async Task Handle_AudiosNotFound_ReturnsFailResult()
        {
            // Arrange
            var query = new GetAllAudiosQuery();

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.GetAllAsync(
                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new List<AudioEntity>());

            var expectedErrorMsg = string.Format(Messages.Error_EntitiesNotFound, nameof(AudioEntity));

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedErrorMsg);
        }

        [Fact]
        public async Task Handle_AudiosExist_ReturnsOkResultWithBase64()
        {
            // Arrange
            var query = new GetAllAudiosQuery();
            var audios = new List<AudioEntity>
            {
                new () { Id = 1, BlobName = "audio1.mp3" },
                new () { Id = 2, BlobName = "audio2.mp3" }
            };

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.GetAllAsync(
                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(audios);

            this.mockBlobService
                .Setup(b => b.FindFileInStorageAsBase64(It.IsAny<string>()))
                .ReturnsAsync((string blobName) => $"base64-content-of-{blobName}");

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);

            result.Value.ElementAt(0).Base64.Should().Be("base64-content-of-audio1.mp3");
            result.Value.ElementAt(1).Base64.Should().Be("base64-content-of-audio2.mp3");
        }

        [Fact]
        public async Task Handle_ReturnsCorrectType()
        {
            // Arrange
            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.GetAllAsync(
                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new List<AudioEntity> { new () });

            this.mockBlobService
                .Setup(b => b.FindFileInStorageAsBase64(It.IsAny<string>()))
                .ReturnsAsync("base64");

            // Act
            var result = await this.handler.Handle(new GetAllAudiosQuery(), CancellationToken.None);

            // Assert
            result.Value.Should().BeAssignableTo<IEnumerable<AudioDTO>>();
        }

        [Fact]
        public async Task Handle_BlobNotFound_ReturnsFailResult()
        {
            // Arrange
            var query = new GetAllAudiosQuery();
            var audios = new List<AudioEntity>
            {
                new () { Id = 1, BlobName = "audio1.mp3" },
                new () { Id = 2, BlobName = "audio2.mp3" },
            };

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.GetAllAsync(
                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(audios);

            this.mockBlobService
                .Setup(b => b.FindFileInStorageAsBase64(It.IsAny<string>()))
                .ReturnsAsync((string blobName) => null);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(Messages.Error_MediaBlobNotFound.Format(
                nameof(AudioEntity),
                audios[0].BlobName!));
        }
    }
}