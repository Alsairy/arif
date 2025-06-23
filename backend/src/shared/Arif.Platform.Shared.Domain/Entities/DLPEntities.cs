using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class DLPScanResult : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public DateTime ScanTime { get; set; }
    public DLPScanStatus Status { get; set; }
    public List<DLPViolation> Violations { get; set; } = new();
    public DLPPolicy AppliedPolicy { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class DLPViolation : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid PolicyId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string ViolationType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DLPSeverity Severity { get; set; }
    public string DetectedContent { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public DLPViolationStatus Status { get; set; }
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
    public List<string> MatchedPatterns { get; set; } = new();
    public double ConfidenceScore { get; set; }
}

public class DLPPolicy : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DLPPolicyType Type { get; set; }
    public bool IsEnabled { get; set; }
    public List<DLPRule> Rules { get; set; } = new();
    public List<string> ApplicableContentTypes { get; set; } = new();
    public DLPAction DefaultAction { get; set; }
    public new DateTime CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public int Priority { get; set; }
}

public class DLPRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DLPRuleType Type { get; set; }
    public List<string> Patterns { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public DLPSeverity Severity { get; set; }
    public DLPAction Action { get; set; }
    public bool IsEnabled { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
    public double ConfidenceThreshold { get; set; }
}

public class DLPMetrics : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateTime MetricsDate { get; set; }
    public int TotalScans { get; set; }
    public int ViolationsDetected { get; set; }
    public int ViolationsBlocked { get; set; }
    public int ViolationsQuarantined { get; set; }
    public int FalsePositives { get; set; }
    public Dictionary<string, int> ViolationsByType { get; set; } = new();
    public Dictionary<string, int> ViolationsBySeverity { get; set; } = new();
    public double AverageConfidenceScore { get; set; }
    public List<DLPTrend> Trends { get; set; } = new();
}

public class DLPTrend
{
    public DateTime Date { get; set; }
    public int ViolationCount { get; set; }
    public string ViolationType { get; set; } = string.Empty;
    public double TrendDirection { get; set; }
}

public class QuarantinedContent : BaseEntity
{
    public Guid TenantId { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime QuarantinedAt { get; set; }
    public string QuarantineReason { get; set; } = string.Empty;
    public Guid QuarantinedBy { get; set; }
    public QuarantineStatus Status { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public string? ReleaseReason { get; set; }
    public Guid? ReleasedBy { get; set; }
    public List<DLPViolation> RelatedViolations { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class DLPIncidentResponse : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid IncidentId { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DLPIncidentSeverity Severity { get; set; }
    public DLPIncidentStatus Status { get; set; }
    public new DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid AssignedTo { get; set; }
    public List<DLPIncidentAction> Actions { get; set; } = new();
    public List<DLPViolation> RelatedViolations { get; set; } = new();
    public string Resolution { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
}

public class DLPIncidentAction
{
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public Guid ExecutedBy { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> ActionData { get; set; } = new();
}

public enum DLPScanStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

public enum DLPSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum DLPViolationStatus
{
    Open,
    InProgress,
    Resolved,
    FalsePositive,
    Accepted
}

public enum DLPPolicyType
{
    ContentInspection,
    PatternMatching,
    MachineLearning,
    Hybrid
}

public enum DLPAction
{
    Allow,
    Block,
    Quarantine,
    Alert,
    Encrypt,
    Redact
}

public enum DLPRuleType
{
    Regex,
    Keyword,
    MachineLearning,
    FingerPrint,
    DocumentMatching
}

public enum QuarantineStatus
{
    Quarantined,
    UnderReview,
    Released,
    Deleted,
    Archived
}

public enum DLPIncidentSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum DLPIncidentStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}
