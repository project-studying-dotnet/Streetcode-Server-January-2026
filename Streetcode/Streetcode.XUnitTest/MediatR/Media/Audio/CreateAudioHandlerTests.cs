using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.Create;
using Streetcode.DAL.Entities.Media;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using FluentResults;
using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

namespace Streetcode.BLL.Test.MediatR.Media.Audio.Create;

public class CreateAudioHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<ILoggerService> _mockLogger;

    public CreateAudioHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockBlobService = new Mock<IBlobService>();
        _mockLogger = new Mock<ILoggerService>();
    }

    [Fact]
    public async Task SuccessfulCreation_ReturnsOkWithAudioDTO()
    {
        var audioCreateDto = new AudioFileBaseCreateDTO { Description = "Test Audio Description" };
        var command = new CreateAudioCommand(audioCreateDto);
        var audioEntity = new AudioEntity { Id = 1 };
        var expectedDto = new AudioDTO { Id = 1, Description = "Test Audio Description" };

        _mockBlobService.Setup(s => s.SaveFileInStorage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("hashed-file-name");

        _mockMapper.Setup(m => m.Map<AudioEntity>(It.IsAny<AudioFileBaseCreateDTO>()))
            .Returns(audioEntity);

        _mockRepositoryWrapper.Setup(r => r.AudioRepository.CreateAsync(It.IsAny<AudioEntity>()))
            .ReturnsAsync(audioEntity);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<AudioDTO>(It.IsAny<AudioEntity>()))
            .Returns(expectedDto);

        var handler = new CreateAudioHandler(
            _mockBlobService.Object,
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedDto.Id, result.Value.Id);
        _mockRepositoryWrapper.Verify(r => r.AudioRepository.CreateAsync(It.IsAny<AudioEntity>()), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DatabaseSaveFails_ReturnsFailAndLogsError()
    {
        // Arrange
        var command = new CreateAudioCommand(new AudioFileBaseCreateDTO());
        var audioEntity = new AudioEntity();

        _mockBlobService.Setup(s => s.SaveFileInStorage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("hashed-name");

        _mockMapper.Setup(m => m.Map<AudioEntity>(It.IsAny<AudioFileBaseCreateDTO>()))
            .Returns(audioEntity);

        _mockRepositoryWrapper.Setup(r => r.AudioRepository.CreateAsync(It.IsAny<AudioEntity>()))
            .ReturnsAsync(audioEntity);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        var handler = new CreateAudioHandler(
            _mockBlobService.Object,
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Failed to create an audio", result.Errors[0].Message);
        _mockLogger.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task BlobService_WithCorrectParameters()
    {
        var audioCreateDto = new AudioFileBaseCreateDTO
        {
            BaseFormat = "mp3",
            Title = "test",
            Extension = "audio"
        };
        var command = new CreateAudioCommand(audioCreateDto);
        var audioEntity = new AudioEntity();

        _mockMapper.Setup(m => m.Map<AudioEntity>(It.IsAny<AudioFileBaseCreateDTO>()))
            .Returns(audioEntity);

        _mockRepositoryWrapper.Setup(r => r.AudioRepository.CreateAsync(It.IsAny<AudioEntity>()))
            .ReturnsAsync(audioEntity);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        var handler = new CreateAudioHandler(
            _mockBlobService.Object,
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);

        await handler.Handle(command, CancellationToken.None);

        _mockBlobService.Verify(s => s.SaveFileInStorage(
            audioCreateDto.BaseFormat,
            audioCreateDto.Title,
            audioCreateDto.Extension), Times.Once);
    }
}