using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Options;
using Moq;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Entities.Media;
using Streetcode.DAL.Entities.Media.Images;
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
    public async Task FindFileInStorageAsMemoryStream_ShouldReturnMemoryStream_WhenFileExists()
    {
        // Arrange
        var originalData = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "existingFile.bin";
        var encrypted = FileService.EncryptBytes(originalData, _testKey);

        File.WriteAllBytes(Path.Combine(_testTempPath, fileName), encrypted);

        // Act
        var result = await _service.FindFileInStorageAsMemoryStream(fileName);

        // Assert
        Assert.NotNull(result);
        result.ToArray().Should().BeEquivalentTo(originalData);
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
    public async Task FindFileInStorageAsBase64_ReturnsNull_WhenFileDoesNotExist()
    {
        // Act
        var result = await _service.FindFileInStorageAsBase64("non-existent.png");

        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task UpdateFileInStorage_ShouldDeleteOldFileAndCreateNewOne()
    {
        // Arrange
        string oldFileName = "old_image.png";
        string newFileName = "updated_profile";
        string extension = "png";
        byte[] newRawBytes = new byte[] { 4, 5, 6 };
        string newBase64 = Convert.ToBase64String(newRawBytes);
        string oldFilePath = Path.Combine(_testTempPath, oldFileName);
        await File.WriteAllBytesAsync(oldFilePath, new byte[] { 1, 1, 1 });

        // Act
        string resultHashName = await _service.UpdateFileInStorage(
            oldFileName, 
            newBase64, 
            newFileName, 
            extension);

        // Assert
        string newFilePath = Path.Combine(_testTempPath, $"{resultHashName}.{extension}");

        Assert.False(File.Exists(oldFilePath), "The old file should have been deleted from storage.");

        Assert.True(File.Exists(newFilePath), "The new file should have been created in storage.");

        byte[] storedBytes = await File.ReadAllBytesAsync(newFilePath);
        Assert.NotEqual(newRawBytes, storedBytes);

        byte[] decryptedBytes = FileService.DecryptBytes(storedBytes, _testKey);
        Assert.Equal(newRawBytes, decryptedBytes);
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

    [Fact]
    public async Task CleanBlobStorage_ShouldDeleteOnlyOrphanedFiles()
    {
        // Arrange
        string fileInDb = "db_linked_file.png";
        string fileOrphan = "orphan_file.png";
        await File.WriteAllBytesAsync(Path.Combine(_testTempPath, fileInDb), new byte[] { 0 }, CancellationToken.None);
        await File.WriteAllBytesAsync(Path.Combine(_testTempPath, fileOrphan), new byte[] { 0 }, CancellationToken.None);

        // 2. Mock the DB to say only 'fileInDb' is used
        _mockRepo.Setup(r => r.ImageRepository.GetAllAsync(
                It.IsAny<Expression<Func<Image, bool>>>(),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(
                new List<Image>
                {
                    new () { BlobName = fileInDb },
                });

        _mockRepo.Setup(r => r.AudioRepository.GetAllAsync(
                It.IsAny<Expression<Func<Audio, bool>>>(),
                It.IsAny<Func<IQueryable<Audio>, IIncludableQueryable<Audio, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new List<Audio>());

        // Act
        await _service.CleanBlobStorage();

        // Assert
        var remainingFiles = Directory.GetFiles(_testTempPath).Select(Path.GetFileName);

        Assert.Contains(fileInDb, remainingFiles);
        Assert.DoesNotContain(fileOrphan, remainingFiles);
    }
}