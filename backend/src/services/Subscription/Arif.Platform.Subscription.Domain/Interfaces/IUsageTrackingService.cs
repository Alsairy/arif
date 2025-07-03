namespace Arif.Platform.Subscription.Domain.Interfaces;

public interface IUsageTrackingService
{
    Task<object> TrackUsageAsync(object request);
    Task<object> GetUsageStatisticsAsync(object request);
    Task<object> GetUsageStatsAsync(Guid tenantId);
    Task<IEnumerable<object>> GetUsageHistoryAsync(Guid tenantId);
    Task<object> GetCurrentUsageAsync(Guid tenantId);
    Task ResetUsageAsync(Guid tenantId);
}
