using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Shared.Services;

namespace Streetcode.BLL.Services.BlobStorageService;

public class BlobService : IBlobService
{
    private readonly string _keyCrypt;
    private readonly string _blobPath;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public BlobService(IOptions<BlobEnvironmentVariables> environment, IRepositoryWrapper? repositoryWrapper = null)
    {
        _keyCrypt = environment.Value.BlobStoreKey;
        _blobPath = environment.Value.BlobStorePath;
        _repositoryWrapper = repositoryWrapper;
    }

    public Task<MemoryStream?> FindFileInStorageAsMemoryStream(string name)
    {
        var decodedBytes = GetDecryptedFile(name);

        if (decodedBytes == null)
        {
            return Task.FromResult<MemoryStream>(null!);
        }

        var image = new MemoryStream(decodedBytes);

        return Task.FromResult(image);
    }

    public Task<string?> FindFileInStorageAsBase64(string name)
    {
        var decodedBytes = GetDecryptedFile(name);

        if (decodedBytes == null)
        {
            return Task.FromResult<string?>(null!);
        }

        string base64 = Convert.ToBase64String(decodedBytes);

        return Task.FromResult(base64);
    }

    public Task<string> SaveFileInStorage(string base64, string name, string extension)
    {
        byte[] imageBytes = Convert.FromBase64String(base64);
        string createdFileName = FileService.PrepareFileStorageName(name);

        string hashBlobStorageName = FileService.HashFunction(createdFileName);

        Directory.CreateDirectory(_blobPath);
        byte[] encryptedData = FileService.EncryptBytes(imageBytes, _keyCrypt);
        File.WriteAllBytes($"{_blobPath}{hashBlobStorageName}.{extension}", encryptedData);

        return Task.FromResult(hashBlobStorageName);
    }

    public Task SaveFileInStorageBase64(string base64, string name, string extension)
    {
        byte[] imageBytes = Convert.FromBase64String(base64);
        Directory.CreateDirectory(_blobPath);
        var encryptedBytes = FileService.EncryptBytes(imageBytes, _keyCrypt);
        File.WriteAllBytes($"{_blobPath}{name}.{extension}", encryptedBytes);
        return Task.CompletedTask;
    }

    public Task DeleteFileInStorage(string name)
    {
        File.Delete($"{_blobPath}{name}");
        return Task.CompletedTask;
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

    public async Task CleanBlobStorage()
    {
        var base64Files = GetAllBlobNames();

        var existingImagesInDatabase = await _repositoryWrapper.ImageRepository.GetAllAsync();
        var existingAudiosInDatabase = await _repositoryWrapper.AudioRepository.GetAllAsync();

        List<string> existingMedia = new ();
        existingMedia.AddRange(existingImagesInDatabase.Select(img => img.BlobName));
        existingMedia.AddRange(existingAudiosInDatabase.Select(img => img.BlobName));

        var filesToRemove = base64Files.Except(existingMedia).ToList();

        foreach (var file in filesToRemove)
        {
            Console.WriteLine($"Deleting {file}...");
            await DeleteFileInStorage(file);
        }
    }

    private IEnumerable<string> GetAllBlobNames()
    {
        var paths = Directory.EnumerateFiles(_blobPath);

        return paths.Select(p => Path.GetFileName(p));
    }

    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:ClosingSquareBracketsMustBeSpacedCorrectly", Justification = "Reviewed.")]
    private byte[]? GetDecryptedFile(string name)
    {
        if (!File.Exists($"{_blobPath}{name}"))
        {
            return null;
        }

        byte[] encryptedData = File.ReadAllBytes($"{_blobPath}{name}");
        return FileService.DecryptBytes(encryptedData, _keyCrypt);
    }
}