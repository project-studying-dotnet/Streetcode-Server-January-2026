using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Media;
using Streetcode.BLL.MediatR.Media.Audio.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Xunit;
using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

namespace Streetcode.XUnitTest.MediatR.Media.Audio.Get 
{

    public class GetAudioByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> mockRepositoryWrapper;
        private readonly Mock<IBlobService> mockBlobService;
        private readonly Mock<ILoggerService> mockLogger;
        private readonly IMapper mapper;
        private readonly GetAudioByStreetcodeIdQueryHandler handler;

        public GetAudioByStreetcodeIdHandlerTests()
        {
            this.mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            this.mockBlobService = new Mock<IBlobService>();
            this.mockLogger = new Mock<ILoggerService>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AudioProfile>();
            });
            this.mapper = new Mapper(configuration);

            this.handler = new GetAudioByStreetcodeIdQueryHandler(
                this.mockRepositoryWrapper.Object,
                this.mapper,
                this.mockBlobService.Object,
                this.mockLogger.Object);
        }

        [Fact]
        public async Task Handle_AudioNotFound_ReturnsFailResult()
        {
            // Arrange
            int streetcodeId = 1;
            var query = new GetAudioByStreetcodeIdQuery(streetcodeId);

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<AudioEntity>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<AudioEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((AudioEntity?)null);

            var expectedErrorMsg = string.Format(Messages.Error_EntityWithStreetcodeIdNotFound, nameof(AudioEntity), streetcodeId);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedErrorMsg);
        }

        [Fact]
        public async Task Handle_AudioExists_ReturnsOkResultWithBase64()
        {
            // Arrange
            int streetcodeId = 1;
            var query = new GetAudioByStreetcodeIdQuery(streetcodeId);
            var audioEntity = new AudioEntity
            {
                Id = 10,
                BlobName = "streetcode-audio.mp3",
            };
            string expectedBase64 = "base64-audio-content";

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<AudioEntity>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<AudioEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(audioEntity);

            this.mockBlobService
                .Setup(b => b.FindFileInStorageAsBase64(audioEntity.BlobName))
                .ReturnsAsync(expectedBase64);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Base64.Should().Be(expectedBase64);
            result.Value.BlobName.Should().Be(audioEntity.BlobName);
        }
    }
}