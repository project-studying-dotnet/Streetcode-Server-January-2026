namespace Streetcode.XUnitTest.MediatR.Media.Audio.Get
{
    using AutoMapper;
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.Interfaces.BlobStorage;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Media;
    using Streetcode.BLL.MediatR.Media.Audio.GetById;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.Resources;
    using System.Linq.Expressions;
    using Xunit;
    using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

    public class GetAudioByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> mockRepositoryWrapper;
        private readonly Mock<IBlobService> mockBlobService;
        private readonly Mock<ILoggerService> mockLogger;
        private readonly IMapper mapper;
        private readonly GetAudioByIdHandler handler;

        public GetAudioByIdHandlerTests()
        {
            this.mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            this.mockBlobService = new Mock<IBlobService>();
            this.mockLogger = new Mock<ILoggerService>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AudioProfile>();
            });
            this.mapper = new Mapper(configuration);

            this.handler = new GetAudioByIdHandler(
                this.mockRepositoryWrapper.Object,
                this.mapper,
                this.mockBlobService.Object,
                this.mockLogger.Object);
        }

        [Fact]
        public async Task Handle_AudioExists_ReturnsOkResultWithAudioDTO()
        {
            // Arrange
            int testId = 1;
            var audioEntity = new AudioEntity
            {
                Id = testId,
                BlobName = "audio-id-1.mp3",
            };
            var query = new GetAudioByIdQuery(testId);
            string expectedBase64 = "base64-encoded-audio";

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
            result.Value.Id.Should().Be(testId);
            result.Value.Base64.Should().Be(expectedBase64);
        }

        [Fact]
        public async Task Handle_AudioDoesNotExist_ReturnsFailResult()
        {
            // Arrange
            int testId = 1;
            var query = new GetAudioByIdQuery(testId);

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<AudioEntity>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<AudioEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((AudioEntity?)null);

            var expectedErrorMsg = string.Format(Messages.Error_EntityWithIdNotFound, nameof(AudioEntity), testId);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedErrorMsg);
        }
    }
}
