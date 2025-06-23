using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class MetricData : BaseEntity
{
    public string ServiceName { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
    public string Unit { get; set; } = string.Empty;
    public MetricType Type { get; set; }
}

public class TraceData : BaseEntity
{
    public string TraceId { get; set; } = string.Empty;
    public string SpanId { get; set; } = string.Empty;
    public string ParentSpanId { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public TraceStatus Status { get; set; }
    public Dictionary<string, object> Tags { get; set; } = new();
    public List<TraceEvent> Events { get; set; } = new();
}

public class TraceEvent
{
    public DateTime Timestamp { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Attributes { get; set; } = new();
}

public class LogEntry : BaseEntity
{
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public string SpanId { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public Exception? Exception { get; set; }
}

public class HealthCheckResult : BaseEntity
{
    public string ServiceName { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public DateTime CheckTime { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public List<HealthCheckDependency> Dependencies { get; set; } = new();
}

public class HealthCheckDependency
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
}



public class DashboardData : BaseEntity
{
    public string DashboardId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
}





public class PerformanceMetric : BaseEntity
{
    public string ServiceName { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Dimensions { get; set; } = new();
    public PerformanceCategory Category { get; set; }
}

public class ErrorMetric : BaseEntity
{
    public string ServiceName { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime Timestamp { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public ErrorSeverity Severity { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

public class SystemOverview : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<ServiceStatus> Services { get; set; } = new();
    public SystemMetrics Metrics { get; set; } = new();
    public List<ActiveAlert> ActiveAlerts { get; set; } = new();
    public ResourceUtilization ResourceUtilization { get; set; } = new();
}

public class ServiceStatus
{
    public string ServiceName { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public DateTime LastCheck { get; set; }
    public TimeSpan Uptime { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public int ActiveConnections { get; set; }
}

public class SystemMetrics
{
    public double TotalRequests { get; set; }
    public double RequestsPerSecond { get; set; }
    public double AverageResponseTime { get; set; }
    public double ErrorRate { get; set; }
    public double ThroughputMbps { get; set; }
    public int ActiveUsers { get; set; }
}

public class ActiveAlert
{
    public string AlertId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public DateTime TriggeredAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
}

public class ResourceUtilization
{
    public double CpuUtilization { get; set; }
    public double MemoryUtilization { get; set; }
    public double DiskUtilization { get; set; }
    public double NetworkUtilization { get; set; }
    public int DatabaseConnections { get; set; }
    public int CacheHitRate { get; set; }
}

public class TraceContext
{
    public string TraceId { get; set; } = string.Empty;
    public string SpanId { get; set; } = string.Empty;
    public string ParentSpanId { get; set; } = string.Empty;
    public Dictionary<string, object> Baggage { get; set; } = new();
    public DateTime StartTime { get; set; }
}

public class TraceSpan
{
    public string SpanId { get; set; } = string.Empty;
    public string ParentSpanId { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Tags { get; set; } = new();
    public List<TraceEvent> Events { get; set; } = new();
}

public class TraceAnalytics
{
    public string TraceId { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public int SpanCount { get; set; }
    public int ServiceCount { get; set; }
    public List<ServiceLatency> ServiceLatencies { get; set; } = new();
    public List<CriticalPath> CriticalPaths { get; set; } = new();
    public List<TraceError> Errors { get; set; } = new();
}

public class ServiceLatency
{
    public string ServiceName { get; set; } = string.Empty;
    public TimeSpan TotalTime { get; set; }
    public double Percentage { get; set; }
    public int SpanCount { get; set; }
}

public class CriticalPath
{
    public List<string> SpanIds { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public double PercentageOfTotal { get; set; }
}

public class TraceError
{
    public string SpanId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ServiceDependency
{
    public string ServiceName { get; set; } = string.Empty;
    public string DependentService { get; set; } = string.Empty;
    public int CallCount { get; set; }
    public TimeSpan AverageLatency { get; set; }
    public double ErrorRate { get; set; }
    public DependencyType Type { get; set; }
}

public enum MetricType
{
    Counter,
    Gauge,
    Histogram,
    Summary
}

public enum TraceStatus
{
    Ok,
    Error,
    Timeout,
    Cancelled
}

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy,
    Unknown
}







public enum PerformanceCategory
{
    ResponseTime,
    Throughput,
    ErrorRate,
    ResourceUtilization,
    DatabasePerformance,
    CachePerformance
}

public enum ErrorSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum DependencyType
{
    Database,
    ExternalApi,
    MessageQueue,
    Cache,
    FileSystem,
    Network
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
