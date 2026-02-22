using Streetcode.BLL.Interfaces.Cache;

namespace Streetcode.BLL.Services.Cache
{
    public class NoCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        {
            return Task.FromResult<T?>(default);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration)
        {
            return Task.CompletedTask;
        }
    }
}
