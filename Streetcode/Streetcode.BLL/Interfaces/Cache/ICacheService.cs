namespace Streetcode.BLL.Interfaces.Cache
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null);
        Task RemoveAsync(string key, CancellationToken cancellationToken);
    }
}
