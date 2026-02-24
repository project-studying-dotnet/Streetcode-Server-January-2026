namespace Streetcode.XUnitTest.MediatR.Media.Audio.Delete
{
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.Interfaces.BlobStorage;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Media.Audio.Delete;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.Resources;
    using System.Linq.Expressions;
    using Xunit;
    using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

    public class DeleteAudioHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> mockRepositoryWrapper;
        private readonly Mock<IBlobService> mockBlobService;
        private readonly Mock<ILoggerService> mockLogger;
        private readonly DeleteAudioHandler handler;

        public DeleteAudioHandlerTests()
        {
            this.mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            this.mockBlobService = new Mock<IBlobService>();
            this.mockLogger = new Mock<ILoggerService>();

            this.handler = new DeleteAudioHandler(
                   this.mockRepositoryWrapper.Object,
                   this.mockBlobService.Object,
                   this.mockLogger.Object);
        }

        [Fact]
        public async Task Handle_AudioNotFound_ReturnsFailResult()
        {
            // Arrange
            int testId = 1;
            var command = new DeleteAudioCommand(testId);

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync((AudioEntity?)null);

            var expectedErrorMsg = string.Format(Messages.Error_EntityWithIdNotFound, nameof(AudioEntity), testId);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedErrorMsg);
        }

        [Fact]
        public async Task Handle_DeleteFromDbSuccessful_DeletesBlobAndReturnsOk()
        {
            // Arrange
            int testId = 1;
            var audioEntity = new AudioEntity
            {
                Id = testId,
                BlobName = "audio-file.mp3",
            };

            var command = new DeleteAudioCommand(testId);

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<AudioEntity, bool>>>(), null, It.IsAny<bool>()))
                .ReturnsAsync(audioEntity);

            this.mockRepositoryWrapper
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            this.mockRepositoryWrapper.Verify(r => r.AudioRepository.Delete(audioEntity), Times.Once);
            this.mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);

            this.mockBlobService.Verify(b => b.DeleteFileInStorage(audioEntity.BlobName), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFromDbFails_ReturnsFailResultAndDoesNotDeleteBlob()
        {
            // Arrange
            int testId = 1;
            var audioEntity = new AudioEntity
            {
                Id = testId,
                BlobName = "audio-file.mp3",
            };
            var command = new DeleteAudioCommand(testId);

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<AudioEntity, bool>>>(), null, It.IsAny<bool>()))
                .ReturnsAsync(audioEntity);

            this.mockRepositoryWrapper
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(0);

            var expectedErrorMsg = string.Format(Messages.Error_FailedToDeleteEntity, nameof(AudioEntity));

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedErrorMsg);
        }
    }
}
