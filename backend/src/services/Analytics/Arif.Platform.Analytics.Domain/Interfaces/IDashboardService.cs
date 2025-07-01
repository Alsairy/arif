namespace Arif.Platform.Analytics.Domain.Interfaces;

public interface IDashboardService
{
    Task<object> GetRealTimeDashboardDataAsync(Guid tenantId);
    Task<object> GetDashboardDataAsync();
    Task<object> GetMetricsAsync();
    Task<object> GetChartsAsync();
    Task<object> GetReportsAsync();
    Task<object> ExportDataAsync(object request);
}
