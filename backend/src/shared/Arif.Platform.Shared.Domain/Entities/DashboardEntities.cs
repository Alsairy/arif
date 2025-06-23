using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class DashboardConfiguration : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    public string CreatedBy { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public DashboardLayout Layout { get; set; } = new();

    public List<DashboardWidget> Widgets { get; set; } = new();

    public Dictionary<string, object> Settings { get; set; } = new();
}

public class DashboardWidget : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public WidgetType Type { get; set; }

    [Required]
    public string DashboardId { get; set; } = string.Empty;

    public WidgetPosition Position { get; set; } = new();

    public WidgetConfiguration Configuration { get; set; } = new();

    public List<DashboardMetric> Metrics { get; set; } = new();

    public DateTime LastUpdated { get; set; }
}

public class DashboardMetric : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public double Value { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; set; }

    [Required]
    public string WidgetId { get; set; } = string.Empty;

    public Dictionary<string, object> Tags { get; set; } = new();

    public MetricTrend Trend { get; set; }
}

public class AlertRule : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string MetricName { get; set; } = string.Empty;

    [Required]
    public AlertCondition Condition { get; set; }

    [Required]
    public double Threshold { get; set; }

    [Required]
    public AlertSeverity Severity { get; set; }

    [Required]
    public string TenantId { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public List<AlertAction> Actions { get; set; } = new();

    public DateTime? LastTriggered { get; set; }

    public int TriggerCount { get; set; }
}

public class Incident : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public IncidentSeverity Severity { get; set; }

    [Required]
    public IncidentStatus Status { get; set; }

    [Required]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    public string ReportedBy { get; set; } = string.Empty;

    public string? AssignedTo { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? Resolution { get; set; }

    public List<IncidentAction> Actions { get; set; } = new();

    public List<string> AffectedServices { get; set; } = new();

    public EscalationLevel EscalationLevel { get; set; }
}

public class IncidentAction : BaseEntity
{
    [Required]
    public string IncidentId { get; set; } = string.Empty;

    [Required]
    public IncidentActionType ActionType { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string PerformedBy { get; set; } = string.Empty;

    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class RequestTrace : BaseEntity
{
    [Required]
    public string CorrelationId { get; set; } = string.Empty;

    [Required]
    public string ServiceName { get; set; } = string.Empty;

    [Required]
    public string OperationName { get; set; } = string.Empty;

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public TimeSpan? Duration { get; set; }

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public Dictionary<string, object> Metadata { get; set; } = new();

    public List<ServiceCall> ServiceCalls { get; set; } = new();
}

public class ServiceCall : BaseEntity
{
    [Required]
    public string CorrelationId { get; set; } = string.Empty;

    [Required]
    public string FromService { get; set; } = string.Empty;

    [Required]
    public string ToService { get; set; } = string.Empty;

    [Required]
    public string Operation { get; set; } = string.Empty;

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public TimeSpan Duration { get; set; }

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class DashboardLayout
{
    public int Columns { get; set; } = 12;
    public int Rows { get; set; } = 8;
    public string Theme { get; set; } = "default";
    public bool AutoRefresh { get; set; } = true;
    public int RefreshInterval { get; set; } = 30;
}

public class WidgetPosition
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 3;
}

public class WidgetConfiguration
{
    public Dictionary<string, object> Settings { get; set; } = new();
    public List<string> DataSources { get; set; } = new();
    public string ChartType { get; set; } = "line";
    public TimeRange TimeRange { get; set; } = new();
}

public class TimeRange
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public string Preset { get; set; } = "last_1h";
}

public class AlertAction
{
    public AlertActionType Type { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class AlertRuleConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public AlertCondition Condition { get; set; }
    public double Threshold { get; set; }
    public AlertSeverity Severity { get; set; }
    public List<AlertAction> Actions { get; set; } = new();
}

public class IncidentCreationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public List<string> AffectedServices { get; set; } = new();
    public string ReportedBy { get; set; } = string.Empty;
}

public enum WidgetType
{
    LineChart,
    BarChart,
    PieChart,
    Gauge,
    Counter,
    Table,
    Heatmap,
    Status
}

public enum MetricTrend
{
    Up,
    Down,
    Stable,
    Unknown
}

public enum AlertCondition
{
    GreaterThan,
    LessThan,
    Equals,
    NotEquals,
    GreaterThanOrEqual,
    LessThanOrEqual
}

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum AlertActionType
{
    Email,
    SMS,
    Webhook,
    Slack,
    Teams
}

public enum IncidentSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum IncidentStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public enum IncidentActionType
{
    Created,
    StatusChanged,
    Assigned,
    NoteAdded,
    Escalated,
    Resolved
}

public enum EscalationLevel
{
    None,
    Level1,
    Level2,
    Level3,
    Executive
}
