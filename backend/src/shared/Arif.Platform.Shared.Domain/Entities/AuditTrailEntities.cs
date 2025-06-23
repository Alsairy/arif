using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class AuditTrailEntry : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public AuditEventType EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public AuditResult Result { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    public string RequestId { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string Source { get; set; } = string.Empty;
}

public class AuditTrailReport : BaseEntity
{
    public Guid TenantId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public AuditReportType ReportType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public Guid GeneratedBy { get; set; }
    public string ReportData { get; set; } = string.Empty;
    public AuditReportFormat Format { get; set; }
    public AuditReportStatus Status { get; set; }
    public List<AuditTrailEntry> Entries { get; set; } = new();
    public AuditReportSummary Summary { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
}

public class AuditReportSummary
{
    public int TotalEntries { get; set; }
    public int SuccessfulActions { get; set; }
    public int FailedActions { get; set; }
    public int SecurityEvents { get; set; }
    public int DataAccessEvents { get; set; }
    public int ConfigurationChanges { get; set; }
    public Dictionary<string, int> EventsByType { get; set; } = new();
    public Dictionary<string, int> EventsByUser { get; set; } = new();
    public Dictionary<string, int> EventsByResource { get; set; } = new();
    public List<AuditTrend> Trends { get; set; } = new();
}

public class AuditTrend
{
    public DateTime Date { get; set; }
    public int EventCount { get; set; }
    public AuditEventType EventType { get; set; }
    public double TrendDirection { get; set; }
}

public class AuditSearchCriteria
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Action { get; set; }
    public string? Resource { get; set; }
    public AuditEventType? EventType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? IpAddress { get; set; }
    public AuditResult? Result { get; set; }
    public string? SessionId { get; set; }
    public string? RequestId { get; set; }
    public Dictionary<string, object> AdditionalFilters { get; set; } = new();
    public int PageSize { get; set; } = 50;
    public int PageNumber { get; set; } = 1;
    public string SortBy { get; set; } = "Timestamp";
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;
}

public class AuditTrailMetrics : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateTime MetricsDate { get; set; }
    public int TotalEvents { get; set; }
    public int SecurityEvents { get; set; }
    public int DataAccessEvents { get; set; }
    public int ConfigurationEvents { get; set; }
    public int FailedEvents { get; set; }
    public int UniqueUsers { get; set; }
    public int UniqueSessions { get; set; }
    public Dictionary<string, int> EventsByHour { get; set; } = new();
    public Dictionary<string, int> EventsByDay { get; set; } = new();
    public Dictionary<string, int> TopUsers { get; set; } = new();
    public Dictionary<string, int> TopResources { get; set; } = new();
    public List<AuditAnomaly> Anomalies { get; set; } = new();
}

public class AuditAnomaly
{
    public string AnomalyType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public AnomalySeverity Severity { get; set; }
    public Dictionary<string, object> AnomalyData { get; set; } = new();
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
}

public class AuditTrailExport : BaseEntity
{
    public Guid TenantId { get; set; }
    public string ExportName { get; set; } = string.Empty;
    public ExportFormat Format { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime RequestedAt { get; set; }
    public Guid RequestedBy { get; set; }
    public ExportStatus Status { get; set; }
    public string? FilePath { get; set; }
    public long? FileSize { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> ExportCriteria { get; set; } = new();
    public string? DownloadUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public enum AuditEventType
{
    Authentication,
    Authorization,
    DataAccess,
    DataModification,
    ConfigurationChange,
    SecurityEvent,
    SystemEvent,
    UserAction,
    AdminAction,
    APICall
}

public enum AuditResult
{
    Success,
    Failure,
    Warning,
    Information
}

public enum AuditReportType
{
    Security,
    Compliance,
    UserActivity,
    SystemActivity,
    DataAccess,
    Configuration,
    Custom
}

public enum AuditReportFormat
{
    JSON,
    XML,
    CSV,
    PDF,
    Excel
}

public enum AuditReportStatus
{
    Pending,
    Generating,
    Completed,
    Failed,
    Expired
}

public enum SortDirection
{
    Ascending,
    Descending
}

public enum AnomalySeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum ExportFormat
{
    JSON,
    CSV,
    XML,
    PDF,
    Excel
}

public enum ExportStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Expired
}
