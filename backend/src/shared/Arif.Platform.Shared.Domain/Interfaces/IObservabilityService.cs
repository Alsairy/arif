using Arif.Platform.Shared.Domain.Entities;

namespace Arif.Platform.Shared.Domain.Interfaces;

public interface IObservabilityService
{
    Task<List<MetricData>> GetMetricsAsync(string serviceName, TimeSpan timeRange, CancellationToken cancellationToken = default);
    Task<List<TraceData>> GetTracesAsync(string traceId, CancellationToken cancellationToken = default);
    Task<List<LogEntry>> GetLogsAsync(LogQuery query, CancellationToken cancellationToken = default);
    Task<HealthCheckResult> GetServiceHealthAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<List<AlertRule>> GetActiveAlertsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> CreateAlertRuleAsync(AlertRule alertRule, CancellationToken cancellationToken = default);
    Task<DashboardData> GetDashboardDataAsync(string dashboardId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(string serviceName, TimeSpan timeRange, CancellationToken cancellationToken = default);
    Task<List<ErrorMetric>> GetErrorMetricsAsync(string serviceName, TimeSpan timeRange, CancellationToken cancellationToken = default);
    Task<SystemOverview> GetSystemOverviewAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface ITelemetryService
{
    Task RecordMetricAsync(string metricName, double value, Dictionary<string, string> tags, CancellationToken cancellationToken = default);
    Task StartTraceAsync(string operationName, string traceId, Dictionary<string, object> context, CancellationToken cancellationToken = default);
    Task EndTraceAsync(string traceId, bool success, Dictionary<string, object> result, CancellationToken cancellationToken = default);
    Task LogEventAsync(LogLevel level, string message, Dictionary<string, object> properties, CancellationToken cancellationToken = default);
    Task RecordExceptionAsync(Exception exception, Dictionary<string, object> context, CancellationToken cancellationToken = default);
    Task<string> GenerateCorrelationIdAsync(CancellationToken cancellationToken = default);
}

public interface IDistributedTracingService
{
    Task<TraceContext> StartTraceAsync(string operationName, TraceContext? parentContext = null, CancellationToken cancellationToken = default);
    Task EndTraceAsync(TraceContext context, bool success, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default);
    Task<List<TraceSpan>> GetTraceSpansAsync(string traceId, CancellationToken cancellationToken = default);
    Task<TraceAnalytics> AnalyzeTraceAsync(string traceId, CancellationToken cancellationToken = default);
    Task<List<ServiceDependency>> GetServiceDependenciesAsync(string serviceName, CancellationToken cancellationToken = default);
}

public class LogQuery
{
    public string ServiceName { get; set; } = string.Empty;
    public LogLevel MinLevel { get; set; } = LogLevel.Information;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string SearchText { get; set; } = string.Empty;
    public Dictionary<string, string> Filters { get; set; } = new();
    public int MaxResults { get; set; } = 1000;
}

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical
}
