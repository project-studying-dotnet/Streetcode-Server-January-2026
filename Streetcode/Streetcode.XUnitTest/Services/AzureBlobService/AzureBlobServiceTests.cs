using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Shared.Services;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Shared.Services;
using Azure.Storage.Blobs.Models;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Streetcode.XUnitTest.Services.AzureBlobService;

public class AzureBlobServiceTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<BlobServiceClient> _blobServiceClientMock;
    private readonly Mock<BlobContainerClient> _containerClientMock;
    private readonly Mock<BlobClient> _blobClientMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly IOptions<AzureBlobEnvironmentVariables> _options;
    private readonly BLL.Services.BlobStorageService.AzureBlobService _service;

    public AzureBlobServiceTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _blobServiceClientMock = new Mock<BlobServiceClient>();
        _containerClientMock = new Mock<BlobContainerClient>();
        _blobClientMock = new Mock<BlobClient>();
        _loggerMock = new Mock<ILoggerService>();

        var envVars = new AzureBlobEnvironmentVariables 
        {
            ContainerName = "test-container",
            EncryptionKey = "test-key-12345678901234567890-32", // Ensure 32 bytes(characters)
        };
        _options = Options.Create(envVars);

        _blobServiceClientMock
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(_containerClientMock.Object);

        _containerClientMock
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(_blobClientMock.Object);

        _service = new BLL.Services.BlobStorageService.AzureBlobService(
            _options, 
            _repositoryWrapperMock.Object,
            _blobServiceClientMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SaveFileInStorage_ShouldReturnHashNameAndUploadBlob()
    {
        // Arrange
        string base64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        string name = "testfile";
        string extension = "jpg";

        // Act
        var result = await _service.SaveFileInStorage(base64, name, extension);

        // Assert
        Assert.False(string.IsNullOrEmpty(result));
        _containerClientMock.Verify(
            x => x.CreateIfNotExistsAsync(default,default, default, default), 
            Times.Once);

        _blobClientMock.Verify(
            x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), default),
            Times.Once);
    }

    [Fact]
    public async Task SaveFileInStorage_ShouldEncryptBeforeUploading()
    {
        // Arrange
        var rawData = new byte[] { 7, 8, 9 };
        var base64 = Convert.ToBase64String(rawData);
        byte[]? uploadedBytes = null;

        _blobClientMock
            .Setup(x => x.UploadAsync(
                It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, bool, CancellationToken>((s, _, _) => 
            {
                using var ms = new MemoryStream();
                s.CopyTo(ms);
                uploadedBytes = ms.ToArray();
            })
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        // Act
        await _service.SaveFileInStorage(base64, "testfile", "png");

        // Assert
        Assert.NotNull(uploadedBytes);
        // Verify it's actually encrypted (the uploaded size should be IV(16) + EncryptedData)
        Assert.NotEqual(rawData, uploadedBytes);
        Assert.True(uploadedBytes.Length > rawData.Length);
    }

    [Fact]
    public async Task DeleteFileInStorage_ShouldCallDeleteIfExists()
    {
        // Arrange
        string fileName = "test-blob-name";

        // Act
        await _service.DeleteFileInStorage(fileName);

        // Assert
        _blobClientMock.Verify(
            x => x.DeleteIfExistsAsync(default, default, default),
            Times.Once);

        _loggerMock.Verify(
            x => x.LogInformation(It.Is<string>(s => s.Contains(fileName))),
            Times.Once);
    }

    [Fact]
    public async Task FindFileInStorageAsBase64_ReturnNull_WhenBlobNotFound()
    {
        // Arrange
        _blobClientMock
            .Setup(x => x.DownloadToAsync(It.IsAny<Stream>()))
            .Throws(new RequestFailedException(404, "Not Found"));

        // Act
        var result = await _service.FindFileInStorageAsBase64("non-existent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindFileInStorageAsMemoryStream_ReturnNull_WhenBlobNotFound()
    {
        // Arrange
        _blobClientMock
            .Setup(x => x.DownloadToAsync(It.IsAny<Stream>()))
            .Throws(new RequestFailedException(404, "Not Found"));

        // Act
        var result = await _service.FindFileInStorageAsMemoryStream("non-existent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindFileInStorageAsBase64_ReturnsOriginalString_WhenBlobExists()
    {
        // Arrange
        var originalBytes = new byte[] { 1, 2, 3, 4, 5 };
        var base64Input = Convert.ToBase64String(originalBytes);

        var encryptedDataWithIv = FileService.EncryptBytes(originalBytes, _options.Value.EncryptionKey);

        _blobClientMock
            .Setup(x => x.DownloadToAsync(It.IsAny<Stream>()))
            .Callback<Stream>(s => s.Write(encryptedDataWithIv, 0, encryptedDataWithIv.Length))
            .ReturnsAsync(Mock.Of<Response>());

        // Act
        var result = await _service.FindFileInStorageAsBase64("some-blob-name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(base64Input, result);
    }

    [Fact]
    public async Task FindFileInStorageAsMemoryStream_ReturnsDecryptedStream_WhenBlobExists()
    {
        // Arrange
        var originalBytes = new byte[] { 10, 20, 30, 40, 50 };
        var key = _options.Value.EncryptionKey;
        var encryptedDataWithIv = FileService.EncryptBytes(originalBytes, key);

        _blobClientMock
            .Setup(x => x.DownloadToAsync(It.IsAny<Stream>()))
            .Callback<Stream>(s => s.Write(encryptedDataWithIv, 0, encryptedDataWithIv.Length))
            .ReturnsAsync(Mock.Of<Response>());

        // Act
        using var resultStream = await _service.FindFileInStorageAsMemoryStream("some-blob-name");

        // Assert
        Assert.NotNull(resultStream);

        byte[] actualBytes = resultStream.ToArray();

        Assert.Equal(originalBytes.Length, actualBytes.Length);
        Assert.Equal(originalBytes, actualBytes);
    }

    [Fact]
    public async Task UpdateFileInStorage_ShouldDeleteOldAndUploadNew()
    {
        // Arrange
        string oldName = "old.png";
        string base64 = Convert.ToBase64String(new byte[] { 4, 5, 6 });

        // Act
        await _service.UpdateFileInStorage(oldName, base64, "newFile", "png");

        // Assert
        _blobClientMock.Verify(
            x => x.DeleteIfExistsAsync(default, default, default),
            Times.Once);

        _blobClientMock.Verify(
            x => x.UploadAsync(It.IsAny<Stream>(), true, default),
            Times.AtLeastOnce);
    }
}