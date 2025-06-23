using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Arif.Platform.Shared.Common.Security;

public class RateLimitingService : IRateLimitingService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RateLimitingService> _logger;

    public RateLimitingService(IDistributedCache cache, ILogger<RateLimitingService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan timeWindow, CancellationToken cancellationToken = default)
    {
        var rateLimitInfo = await GetRateLimitInfoAsync(key, maxRequests, timeWindow, cancellationToken);
        return rateLimitInfo.IsAllowed;
    }

    public async Task<RateLimitInfo> GetRateLimitInfoAsync(string key, int maxRequests, TimeSpan timeWindow, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"rate_limit:{key}";
        var now = DateTime.UtcNow;

        try
        {
            var cachedDataJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            RateLimitData? cachedData = null;

            if (!string.IsNullOrEmpty(cachedDataJson))
            {
                cachedData = JsonSerializer.Deserialize<RateLimitData>(cachedDataJson);
            }

            if (cachedData == null || now >= cachedData.ResetTime)
            {
                cachedData = new RateLimitData
                {
                    RequestCount = 1,
                    ResetTime = now.Add(timeWindow)
                };

                var newDataJson = JsonSerializer.Serialize(cachedData);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = cachedData.ResetTime
                };

                await _cache.SetStringAsync(cacheKey, newDataJson, cacheOptions, cancellationToken);

                return new RateLimitInfo
                {
                    IsAllowed = true,
                    RequestsRemaining = maxRequests - 1,
                    ResetTime = cachedData.ResetTime,
                    RetryAfter = TimeSpan.Zero
                };
            }

            if (cachedData.RequestCount >= maxRequests)
            {
                return new RateLimitInfo
                {
                    IsAllowed = false,
                    RequestsRemaining = 0,
                    ResetTime = cachedData.ResetTime,
                    RetryAfter = cachedData.ResetTime - now
                };
            }

            cachedData.RequestCount++;
            var updatedDataJson = JsonSerializer.Serialize(cachedData);
            var updateCacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = cachedData.ResetTime
            };

            await _cache.SetStringAsync(cacheKey, updatedDataJson, updateCacheOptions, cancellationToken);

            return new RateLimitInfo
            {
                IsAllowed = true,
                RequestsRemaining = maxRequests - cachedData.RequestCount,
                ResetTime = cachedData.ResetTime,
                RetryAfter = TimeSpan.Zero
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for key: {Key}", key);
            
            return new RateLimitInfo
            {
                IsAllowed = true,
                RequestsRemaining = maxRequests,
                ResetTime = now.Add(timeWindow),
                RetryAfter = TimeSpan.Zero
            };
        }
    }

    public async Task ResetAsync(string key, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"rate_limit:{key}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);
    }

    private class RateLimitData
    {
        public int RequestCount { get; set; }
        public DateTime ResetTime { get; set; }
    }
}
