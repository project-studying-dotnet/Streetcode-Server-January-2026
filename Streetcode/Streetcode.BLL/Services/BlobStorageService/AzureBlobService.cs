using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Shared.Services;

namespace Streetcode.BLL.Services.BlobStorageService;

public class AzureBlobService : IBlobService
{
    private readonly AzureBlobEnvironmentVariables _options;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILoggerService _logger;

    public AzureBlobService(
        IOptions<AzureBlobEnvironmentVariables> options,
        IRepositoryWrapper repositoryWrapper,
        BlobServiceClient blobServiceClient,
        ILoggerService logger)
    {
        _options = options.Value;
        _repositoryWrapper = repositoryWrapper;
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<string> SaveFileInStorage(string base64, string name, string extension)
    {
        byte[] imageBytes = Convert.FromBase64String(base64);
        string createdFileName = FileService.PrepareFileStorageName(name);

        string hashBlobName = FileService.HashFunction(createdFileName);
        string fullBlobName = $"{hashBlobName}.{extension}";

        byte[] encryptedData = FileService.EncryptBytes(imageBytes, _options.EncryptionKey);

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
        _logger.LogInformation($"Deleting {name} from Azure...");
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task CleanBlobStorage()
    {
        const int BatchSize = 250;
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);

        List<string> currentBatch = new ();

        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            currentBatch.Add(blobItem.Name);

            // When we hit our limit, process the batch
            if (currentBatch.Count >= BatchSize)
            {
                await ProcessBatch(currentBatch, containerClient);
                currentBatch.Clear();
            }
        }

        if (currentBatch.Any())
        {
            await ProcessBatch(currentBatch, containerClient);
        }
    }

    private async Task ProcessBatch(List<string> blobNames, BlobContainerClient container)
    {
        try
        {
            var foundInImages = (await _repositoryWrapper.ImageRepository
                    .GetAllAsync(img => blobNames.Contains(img.BlobName)))
                .Select(img => img.BlobName)
                .ToList();

            var foundInAudios = (await _repositoryWrapper.AudioRepository
                    .GetAllAsync(aud => blobNames.Contains(aud.BlobName)))
                .Select(aud => aud.BlobName)
                .ToList();

            var existingInDb = foundInImages.Concat(foundInAudios).ToHashSet();

            var orphans = blobNames.Except(existingInDb);

            foreach (var orphan in orphans)
            {
                try
                {
                    _logger.LogInformation($"Deleting orphaned blob: {orphan}");
                    await container.GetBlobClient(orphan).DeleteIfExistsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to delete blob {orphan}. Skipping...");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during blob cleanup: {ex.Message}");
        }
    }

    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:ClosingSquareBracketsMustBeSpacedCorrectly", Justification = "Reviewed.")]
    private async Task<byte[]?> FindFileInStorageAsBytes(string name)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);

        var blobClient = containerClient.GetBlobClient(name);

        try
        {
            using var ms = new MemoryStream();
            await blobClient.DownloadToAsync(ms);
            byte[] encryptedBytes = ms.ToArray();

            return FileService.DecryptBytes(encryptedBytes, _options.EncryptionKey);
        }
        catch (RequestFailedException ex)
        {
            if (ex.Status == 404)
            {
                return null;
            }

            throw;
        }
    }
}