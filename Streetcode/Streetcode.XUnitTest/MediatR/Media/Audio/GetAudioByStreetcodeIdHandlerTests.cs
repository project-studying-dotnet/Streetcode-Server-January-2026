using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.GetByStreetcodeId;
using Streetcode.DAL.Entities.Media;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

namespace Streetcode.XUnitTest.MediatR.Media.Audio.GetByStreetcodeId;

public class GetAudioByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlob;
    private readonly Mock<ILoggerService> _mockLogger;

    public GetAudioByStreetcodeIdHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockBlob = new Mock<IBlobService>();
        _mockLogger = new Mock<ILoggerService>();
    }

    [Fact]
    public async Task ExistingAudio_ReturnsSuccessWithBase64()
    {
        int streetcodeId = 1;

        var audio = new AudioEntity { Id = 1, BlobName = "audio.mp3" };
        var streetcode = new StreetcodeContent { Id = streetcodeId, Audio = audio };
        var audioDto = new AudioDTO { Id = 1, BlobName = "audio.mp3" };
        string expectedBase64 = "base64string";

        _mockRepo.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync(streetcode);

        _mockMapper.Setup(m => m.Map<AudioDTO>(It.IsAny<AudioEntity>())).Returns(audioDto);
        _mockBlob.Setup(b => b.FindFileInStorageAsBase64(audioDto.BlobName)).Returns(expectedBase64);

        var handler = new GetAudioByStreetcodeIdQueryHandler(_mockRepo.Object, _mockMapper.Object, _mockBlob.Object, _mockLogger.Object);
        var query = new GetAudioByStreetcodeIdQuery(streetcodeId);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedBase64, result.Value.Base64);
        _mockBlob.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Once);
    }
}