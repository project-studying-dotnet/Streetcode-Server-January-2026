namespace Streetcode.XUnitTest.MediatR.Media.Audio.Get
{
    using System.Linq.Expressions;
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.Interfaces.BlobStorage;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Media.Audio.GetBaseAudio;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.Resources;
    using Xunit;
    using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

    public class GetBaseAudioHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> mockRepositoryWrapper;
        private readonly Mock<IBlobService> mockBlobService;
        private readonly Mock<ILoggerService> mockLogger;
        private readonly GetBaseAudioHandler handler;

        public GetBaseAudioHandlerTests()
        {
            this.mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            this.mockBlobService = new Mock<IBlobService>();
            this.mockLogger = new Mock<ILoggerService>();

            this.handler = new GetBaseAudioHandler(
                this.mockBlobService.Object,
                this.mockRepositoryWrapper.Object,
                this.mockLogger.Object);
        }

        [Fact]
        public async Task Handle_AudioExists_ReturnsMemoryStream()
        {
            // Arrange
            int testId = 1;
            var audioEntity = new AudioEntity { Id = testId, BlobName = "test-audio.mp3" };
            var query = new GetBaseAudioQuery(testId);

            var memoryStream = new MemoryStream(new byte[] { 1, 2, 3 });

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<AudioEntity>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<AudioEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(audioEntity);

            this.mockBlobService
                .Setup(b => b.FindFileInStorageAsMemoryStream(audioEntity.BlobName))
                .Returns(memoryStream);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeSameAs(memoryStream);
        }

        [Fact]
        public async Task Handle_AudioNotFound_ReturnsFailResult()
        {
            // Arrange
            int testId = 1;
            var query = new GetBaseAudioQuery(testId);

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