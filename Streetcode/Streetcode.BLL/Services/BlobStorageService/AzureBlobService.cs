using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Shared.Services;

namespace Streetcode.BLL.Services.BlobStorageService;

public class AzureBlobService : IBlobService
{
    private readonly AzureBlobEnvironmentVariables _options;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobService(
        IOptions<AzureBlobEnvironmentVariables> options,
        IRepositoryWrapper repositoryWrapper,
        BlobServiceClient blobServiceClient)
    {
        _options = options.Value;
        _repositoryWrapper = repositoryWrapper;
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> SaveFileInStorage(string base64, string name, string extension)
    {
        byte[] imageBytes = Convert.FromBase64String(base64);
        string createdFileName = FileService.PrepareFileStorageName(name);

        string hashBlobName = FileService.HashFunction(createdFileName);
        string fullBlobName = $"{hashBlobName}.{extension}";

        byte[] encryptedData = FileService.EncryptFile(imageBytes, _options.EncryptionKey);

        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(fullBlobName);

        using (var ms = new MemoryStream(encryptedData))
        {
            await blobClient.UploadAsync(ms, overwrite: true);
        }

        return hashBlobName;
    }

    public async Task<MemoryStream?> FindFileInStorageAsMemoryStream(string name)
    {
        var bytes = await FindFileInStorageAsBytes(name);
        return bytes is null ? null : new MemoryStream(bytes);
    }

    public async Task<string?> FindFileInStorageAsBase64(string name)
    {
        var bytes = await FindFileInStorageAsBytes(name);
        return bytes is null ? null : Convert.ToBase64String(bytes);
    }

    public async Task<string> UpdateFileInStorage(
        string previousBlobName,
        string base64Format,
        string newBlobName,
        string extension)
    {
        await DeleteFileInStorage(previousBlobName);

        string hashBlobStorageName = await SaveFileInStorage(
            base64Format,
            newBlobName,
            extension);

        return hashBlobStorageName;
    }

    public async Task DeleteFileInStorage(string name)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        BlobClient blobClient = containerClient.GetBlobClient(name);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task CleanBlobStorage()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);

        List<string> blobsInAzure = new ();
        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            blobsInAzure.Add(blobItem.Name);
        }

        var existingImages = await _repositoryWrapper.ImageRepository.GetAllAsync();
        var existingAudios = await _repositoryWrapper.AudioRepository.GetAllAsync();

        var existingMedia = existingImages.Select(img => img.BlobName)
            .Concat(existingAudios.Select(aud => aud.BlobName))
            .ToHashSet(); // HashSet makes the lookup/Except logic much faster

        var filesToRemove = blobsInAzure.Except(existingMedia).ToList();

        foreach (var fileName in filesToRemove)
        {
            Console.WriteLine($"Deleting {fileName} from Azure...");
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }

    private async Task<byte[]> FindFileInStorageAsBytes(string name)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);

        var blobClient = containerClient.GetBlobClient(name);

        try
        {
            using var ms = new MemoryStream();
            await blobClient.DownloadToAsync(ms);
            byte[] encryptedBytes = ms.ToArray();

            return FileService.DecryptFile(encryptedBytes, _options.EncryptionKey);
        }
        catch (RequestFailedException ex)
        {
            if (ex.Status == 404)
            {
                return null!;
            }

            throw;
        }
    }
}