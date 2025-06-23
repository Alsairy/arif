namespace Arif.Platform.Shared.Common.Security;

public interface IRateLimitingService
{
    Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan timeWindow, CancellationToken cancellationToken = default);
    Task<RateLimitInfo> GetRateLimitInfoAsync(string key, int maxRequests, TimeSpan timeWindow, CancellationToken cancellationToken = default);
    Task ResetAsync(string key, CancellationToken cancellationToken = default);
}

public class RateLimitInfo
{
    public bool IsAllowed { get; set; }
    public int RequestsRemaining { get; set; }
    public DateTime ResetTime { get; set; }
    public TimeSpan RetryAfter { get; set; }
}

public class RateLimitOptions
{
    public int MaxRequests { get; set; } = 100;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(1);
    public string KeyPrefix { get; set; } = "rate_limit";
}
