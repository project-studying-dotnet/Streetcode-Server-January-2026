using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Streetcode.BLL.Interfaces.Cache;

namespace Streetcode.BLL.Services.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

        public RedisCacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        {
            var cachedData = await _distributedCache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(cachedData))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedData);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null)
        {
            string serializedData = JsonSerializer.Serialize(value);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? DefaultExpiration
            };

            await _distributedCache.SetStringAsync(key, serializedData, options);
        }
    }
}
