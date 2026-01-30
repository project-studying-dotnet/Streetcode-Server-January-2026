using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

namespace Streetcode.XUnitTest.MediatR.Media.Audio.GetAll;

public class GetAllAudiosHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlob;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetAllAudiosHandler _handler;

    public GetAllAudiosHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockBlob = new Mock<IBlobService>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new GetAllAudiosHandler(
            _mockRepo.Object,
            _mockMapper.Object,
            _mockBlob.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AudiosIsNull_ReturnsError()
    {
        _mockRepo.Setup(x => x.AudioRepository.GetAllAsync(It.IsAny<Expression<Func<AudioEntity, bool>>>(), null))
            .ReturnsAsync((IEnumerable<AudioEntity>?)null);

        var query = new GetAllAudiosQuery();
        const string expectedErrorMessage = "Cannot find any audios";

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(expectedErrorMessage);

        _mockLogger.Verify(x => x.LogError(query, expectedErrorMessage), Times.Once);
    }

    [Fact]
    public async Task AudiosExist_WithBase64Data_ReturnsOk()
    {
        var audios = new List<AudioEntity>
        {
            new() { Id = 1, BlobName = "audio1" },
            new() { Id = 2, BlobName = "audio2" }
        };

        var audioDtos = new List<AudioDTO>
        {
            new() { Id = 1, BlobName = "audio1" },
            new() { Id = 2, BlobName = "audio2" }
        };

        _mockRepo.Setup(x => x.AudioRepository.GetAllAsync(It.IsAny<Expression<Func<AudioEntity, bool>>>(), null))
            .ReturnsAsync(audios);

        _mockMapper.Setup(x => x.Map<IEnumerable<AudioDTO>>(It.IsAny<IEnumerable<AudioEntity>>()))
            .Returns(audioDtos);

        _mockBlob.Setup(x => x.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns("base64-content");

        var result = await _handler.Handle(new GetAllAudiosQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        result.Value.All(a => a.Base64 == "base64-content").Should().BeTrue();

        _mockBlob.Verify(x => x.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Exactly(audios.Count));
    }

    [Fact]
    public async Task NoAudiosInDatabase_ReturnsEmptyList()
    {
        _mockRepo.Setup(x => x.AudioRepository.GetAllAsync(It.IsAny<Expression<Func<AudioEntity, bool>>>(), null))
            .ReturnsAsync(new List<AudioEntity>());

        _mockMapper.Setup(x => x.Map<IEnumerable<AudioDTO>>(It.IsAny<IEnumerable<AudioEntity>>()))
            .Returns(new List<AudioDTO>());

        var result = await _handler.Handle(new GetAllAudiosQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}