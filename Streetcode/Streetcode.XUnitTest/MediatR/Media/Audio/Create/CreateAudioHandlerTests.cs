namespace Streetcode.XUnitTest.MediatR.Media.Audio.Create
{
    using AutoMapper;
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.DTO.Media.Audio;
    using Streetcode.BLL.Interfaces.BlobStorage;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Media;
    using Streetcode.BLL.MediatR.Media.Audio.Create;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.Resources;
    using Xunit;
    using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

    public class CreateAudioHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> mockRepositoryWrapper;
        private readonly Mock<IBlobService> mockBlobService;
        private readonly Mock<ILoggerService> mockLogger;
        private readonly IMapper mapper;
        private readonly CreateAudioHandler handler;

        public CreateAudioHandlerTests()
        {
            this.mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            this.mockBlobService = new Mock<IBlobService>();
            this.mockLogger = new Mock<ILoggerService>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AudioProfile>();
            });

            this.mapper = new Mapper(configuration);

            this.handler = new CreateAudioHandler(
                this.mockBlobService.Object,
                this.mockRepositoryWrapper.Object,
                this.mapper,
                this.mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ValidData_ReturnsOkResult()
        {
            // Arrange
            var createDto = new AudioFileBaseCreateDTO
            {
                Title = "TestAudio",
                Extension = "mp3",
                BaseFormat = "data:audio/mp3;base64,..."
            };
            var command = new CreateAudioCommand(createDto);
            var hashName = "hashed-file";

            this.mockBlobService
                .Setup(b => b.SaveFileInStorage(createDto.BaseFormat, createDto.Title, createDto.Extension))
                .ReturnsAsync(hashName);

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.CreateAsync(It.IsAny<AudioEntity>()))
                .ReturnsAsync((AudioEntity audio) => audio);

            this.mockRepositoryWrapper
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.BlobName.Should().Be($"{hashName}.mp3");
        }

        [Fact]
        public async Task Handle_BlobServiceFails_ThrowsException()
        {
            // Arrange
            var createDto = new AudioFileBaseCreateDTO { Title = "Fail", Extension = "wav", BaseFormat = "" };
            var command = new CreateAudioCommand(createDto);

            this.mockBlobService
                .Setup(b => b.SaveFileInStorage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Blob storage error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => this.handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DatabaseSaveFails_ReturnsFailResult()
        {
            // Arrange
            var createDto = new AudioFileBaseCreateDTO
            {
                Title = "Test",
                Extension = "mp3",
                BaseFormat = "base64",
            };
            var command = new CreateAudioCommand(createDto);

            this.mockBlobService
                .Setup(b => b.SaveFileInStorage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("some-hash");

            this.mockRepositoryWrapper
                .Setup(r => r.AudioRepository.CreateAsync(It.IsAny<AudioEntity>()))
                .ReturnsAsync(new AudioEntity());

            this.mockRepositoryWrapper
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(0);

            var expectedErrorMsg = string.Format(Messages.Error_FailedToCreateEntity, nameof(AudioEntity));

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedErrorMsg);
        }
    }
}