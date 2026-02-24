using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.GetBaseAudio;
using Streetcode.DAL.Entities.Media;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Media.Audio.Get;

public class GetBaseAudioHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<ILoggerService> _mockLogger;

    public GetBaseAudioHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockBlobService = new Mock<IBlobService>();
        _mockLogger = new Mock<ILoggerService>();
    }

    [Fact]
    public async Task AudioExists_ReturnsSuccessWithMemoryStream()
    {
        var audioId = 1;
        var blobName = "test-audio.mp3";
        var expectedStream = new MemoryStream();
        var request = new GetBaseAudioQuery(audioId);

        //_mockRepositoryWrapper
        //    .Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
        //        It.IsAny<Expression<Func<DAL.Entities.Media.Audio, bool>>>(),
        //        null))
        //    .ReturnsAsync(new DAL.Entities.Media.Audio { Id = audioId, BlobName = blobName });

        _mockBlobService
            .Setup(b => b.FindFileInStorageAsMemoryStream(blobName))
            .Returns(expectedStream);

        var handler = new GetBaseAudioHandler(_mockBlobService.Object, _mockRepositoryWrapper.Object, _mockLogger.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(expectedStream);
    }

    [Fact]
    public async Task Handle_AudioDoesNotExist_ReturnsFailAndLogsError()
    {
        var audioId = 1;
        var request = new GetBaseAudioQuery(audioId);

        //_mockRepositoryWrapper
        //    .Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
        //        It.IsAny<Expression<Func<DAL.Entities.Media.Audio, bool>>>(),
        //        null))
        //    .ReturnsAsync((DAL.Entities.Media.Audio)null!);

        var handler = new GetBaseAudioHandler(_mockBlobService.Object, _mockRepositoryWrapper.Object, _mockLogger.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain($"Cannot find an audio with corresponding id: {audioId}");

        _mockLogger.Verify(
            x => x.LogError(request, It.Is<string>(s => s.Contains(audioId.ToString()))),
            Times.Once);
    }
}