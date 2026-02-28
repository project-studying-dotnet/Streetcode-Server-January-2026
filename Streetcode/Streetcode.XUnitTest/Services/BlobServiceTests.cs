using Microsoft.Extensions.Options;
using Moq;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Shared.Services;
using Xunit;

public class BlobServiceTests : IDisposable
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly string _testTempPath;
    private readonly string _testKey = "12345678901234561234567890123456"; // 32 bytes for AES
    private readonly BlobService _service;

    public BlobServiceTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();

        // Create a unique temp folder for each test run to avoid collision
        _testTempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "BlobStore/");
        Directory.CreateDirectory(_testTempPath);

        var options = Options.Create(new BlobEnvironmentVariables
        {
            BlobStoreKey = _testKey,
            BlobStorePath = _testTempPath
        });

        _service = new BlobService(options, _mockRepo.Object);
    }

    // Cleanup after every test
    public void Dispose()
    {
        if (Directory.Exists(_testTempPath))
        {
            Directory.Delete(_testTempPath, true);
        }
    }

    [Fact]
    public async Task SaveFileInStorage_ShouldCreateFileOnDisk()
    {
        // Arrange
        var content = "Hello World";
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        var fileName = await _service.SaveFileInStorage(base64, "testfile", "txt");

        // Assert
        var expectedPath = Path.Combine(_testTempPath, $"{fileName}.txt");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public async Task FindFileInStorageAsBase64_ShouldReturnCorrectString_WhenFileExists()
    {
        // Arrange
        var originalData = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "existingFile.bin";
        var encrypted = FileService.EncryptBytes(originalData, _testKey);

        File.WriteAllBytes(Path.Combine(_testTempPath, fileName), encrypted);

        // Act
        var result = await _service.FindFileInStorageAsBase64(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Convert.ToBase64String(originalData), result);
    }

    [Fact]
    public async Task FindFileInStorageAsMemoryStream_ReturnsNull_WhenFileDoesNotExist()
    {
        // Act
        var result = await _service.FindFileInStorageAsMemoryStream("non-existent.png");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteFileInStorage_ShouldRemoveFileFromDisk()
    {
        // Arrange
        var fileName = "toDelete.txt";
        var fullPath = Path.Combine(_testTempPath, fileName);
        File.WriteAllText(fullPath, "content");

        // Act
        await _service.DeleteFileInStorage(fileName);

        // Assert
        Assert.False(File.Exists(fullPath));
    }
}