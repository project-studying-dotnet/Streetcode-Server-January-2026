namespace Streetcode.BLL.Interfaces.BlobStorage;

public interface IBlobService
{
    public Task<string> SaveFileInStorage(string base64, string name, string mimeType);
    public Task<string> UpdateFileInStorage(
        string previousBlobName,
        string base64Format,
        string newBlobName,
        string extension);
    public Task<MemoryStream?> FindFileInStorageAsMemoryStream(string name);
    public Task<string?> FindFileInStorageAsBase64(string name);
    public Task DeleteFileInStorage(string name);
    public Task CleanBlobStorage();
}
