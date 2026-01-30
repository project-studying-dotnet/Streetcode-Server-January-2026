namespace Streetcode.XUnitTest.MediatR.Media.Audio
{
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.Interfaces.BlobStorage;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Media.Audio.Delete;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using System.Linq.Expressions;
    using System.Reflection.Metadata;
    using Xunit;
    using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

    public class DeleteAudioHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repoMock;
        private readonly Mock<IBlobService> _blobMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly DeleteAudioHandler _handler;

        public DeleteAudioHandlerTests()
        {
            _repoMock = new Mock<IRepositoryWrapper>();
            _blobMock = new Mock<IBlobService>();
            _loggerMock = new Mock<ILoggerService>();

            _handler = new DeleteAudioHandler(_repoMock.Object, _blobMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task AudioNotFound_ReturnsFail()
        {
            _repoMock.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                null))
                .ReturnsAsync((AudioEntity?)null);

            var result = await _handler.Handle(new DeleteAudioCommand(1), default);

            result.IsSuccess.Should().BeFalse();
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeletesFromDbAndStorage_Success()
        {
            AudioEntity audio = new AudioEntity { Id = 1, BlobName = "test-blob" };

            _repoMock.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                null))
                .ReturnsAsync(audio);

            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _handler.Handle(new DeleteAudioCommand(1), default);

            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.AudioRepository.Delete(audio), Times.Once);
            _blobMock.Verify(b => b.DeleteFileInStorage(audio.BlobName), Times.Once);
        }

        [Fact]
        public async Task DbSaveFails_ReturnsFail()
        {
            AudioEntity audio = new AudioEntity { Id = 1, BlobName = "test-blob" };

            _repoMock.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                null))
                .ReturnsAsync(audio);

            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            var result = await _handler.Handle(new DeleteAudioCommand(1), default);

            result.IsSuccess.Should().BeFalse();
            _blobMock.Verify(b => b.DeleteFileInStorage(It.IsAny<string>()), Times.Never);
        }
    }
}
