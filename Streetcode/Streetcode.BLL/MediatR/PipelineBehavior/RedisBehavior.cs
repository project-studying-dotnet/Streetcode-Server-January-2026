using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Streetcode.BLL.Interfaces.Cache;

namespace Streetcode.BLL.MediatR.PipelineBehavior
{
    public class RedisBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
         where TRequest : IRequest<TResponse>
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger _logger;

        public RedisBehavior(ICacheService cacheService, ILogger<RedisBehavior<TRequest, TResponse>> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICachableQuery cacheableRequest || cacheableRequest.BypassCache)
            {
                return await next();
            }

            var cacheKey = cacheableRequest.CacheKey.ToLower();

            var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);

            if (cachedResponse != null)
            {
                _logger.LogInformation("Fetched from cache: {CacheKey}", cacheKey);
                return cachedResponse;
            }

            var response = await next();

            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = cacheableRequest.SlidingExpiration,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            var expiration = cacheableRequest.SlidingExpiration;
            await _cacheService.SetAsync(cacheKey, response, expiration, cancellationToken);

            return response;
        }
    }
}