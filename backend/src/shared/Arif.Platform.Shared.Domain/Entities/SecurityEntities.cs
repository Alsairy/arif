using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class SecurityScanResult : BaseEntity
{
    public Guid TenantId { get; set; }
    public ScanType ScanType { get; set; }
    public DateTime ScanStartTime { get; set; }
    public DateTime ScanEndTime { get; set; }
    public ScanStatus Status { get; set; }
    public string ScanTarget { get; set; } = string.Empty;
    public List<SecurityVulnerability> Vulnerabilities { get; set; } = new();
    public SecurityScanSummary Summary { get; set; } = new();
    public string RawResults { get; set; } = string.Empty;
    public string ScannerVersion { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SecurityVulnerability : BaseEntity
{
    public Guid TenantId { get; set; }
    public string VulnerabilityId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public VulnerabilitySeverity Severity { get; set; }
    public string Category { get; set; } = string.Empty;
    public string AffectedComponent { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public VulnerabilityStatus Status { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
    public List<string> References { get; set; } = new();
    public double CVSSScore { get; set; }
    public string CVSSVector { get; set; } = string.Empty;
    public List<RemediationAction> RecommendedActions { get; set; } = new();
}

public class SecurityRiskAssessment : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public string AssessedBy { get; set; } = string.Empty;
    public double OverallRiskScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public List<SecurityRisk> IdentifiedRisks { get; set; } = new();
    public List<SecurityControl> ExistingControls { get; set; } = new();
    public List<SecurityRecommendation> Recommendations { get; set; } = new();
    public string ExecutiveSummary { get; set; } = string.Empty;
}

public class SecurityRisk : BaseEntity
{
    public string RiskId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Likelihood { get; set; }
    public double Impact { get; set; }
    public double RiskScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public List<string> ThreatSources { get; set; } = new();
    public List<string> Vulnerabilities { get; set; } = new();
    public List<SecurityControl> MitigatingControls { get; set; } = new();
}

public class SecurityControl : BaseEntity
{
    public string ControlId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecurityControlType Type { get; set; }
    public SecurityControlStatus Status { get; set; }
    public double Effectiveness { get; set; }
    public DateTime LastReviewed { get; set; }
    public string Owner { get; set; } = string.Empty;
}

public class SecurityRecommendation : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecurityRecommendationPriority Priority { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> ActionItems { get; set; } = new();
    public DateTime RecommendedAt { get; set; }
    public SecurityRecommendationStatus Status { get; set; }
    public DateTime? ImplementedAt { get; set; }
    public string? ImplementationNotes { get; set; }
    public double EstimatedEffort { get; set; }
    public double RiskReduction { get; set; }
}

public class SecurityScanHistory : BaseEntity
{
    public Guid TenantId { get; set; }
    public List<SecurityScanResult> ScanResults { get; set; } = new();
    public SecurityTrendAnalysis TrendAnalysis { get; set; } = new();
    public DateTime LastScanDate { get; set; }
    public DateTime NextScheduledScan { get; set; }
}

public class SecurityTrendAnalysis
{
    public List<SecurityTrend> VulnerabilityTrends { get; set; } = new();
    public List<SecurityTrend> RiskTrends { get; set; } = new();
    public double TrendDirection { get; set; }
    public string TrendSummary { get; set; } = string.Empty;
}

public class SecurityTrend
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class SecurityPosture : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public double OverallScore { get; set; }
    public SecurityPostureLevel Level { get; set; }
    public Dictionary<string, double> CategoryScores { get; set; } = new();
    public List<SecurityMetric> Metrics { get; set; } = new();
    public List<SecurityAlert> ActiveAlerts { get; set; } = new();
    public SecurityPostureHistory History { get; set; } = new();
}

public class SecurityMetric
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime MeasuredAt { get; set; }
    public SecurityMetricStatus Status { get; set; }
}

public class SecurityPostureHistory
{
    public List<SecurityPostureSnapshot> Snapshots { get; set; } = new();
    public double ImprovementRate { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
}

public class SecurityPostureSnapshot
{
    public DateTime Date { get; set; }
    public double Score { get; set; }
    public SecurityPostureLevel Level { get; set; }
}

public class SecurityAlert : BaseEntity
{
    public Guid TenantId { get; set; }
    public string AlertId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecurityAlertSeverity Severity { get; set; }
    public SecurityAlertType Type { get; set; }
    public SecurityAlertStatus Status { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public string? ResolvedBy { get; set; }
    public Dictionary<string, object> AlertData { get; set; } = new();
    public List<SecurityAlertAction> Actions { get; set; } = new();
}

public class SecurityAlertAction
{
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public string ExecutedBy { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SecurityScanSummary
{
    public int TotalVulnerabilities { get; set; }
    public int CriticalVulnerabilities { get; set; }
    public int HighVulnerabilities { get; set; }
    public int MediumVulnerabilities { get; set; }
    public int LowVulnerabilities { get; set; }
    public double AverageRiskScore { get; set; }
    public TimeSpan ScanDuration { get; set; }
    public Dictionary<string, int> VulnerabilitiesByCategory { get; set; } = new();
}

public class PenetrationTestConfig
{
    public string TestName { get; set; } = string.Empty;
    public List<string> TargetSystems { get; set; } = new();
    public PenetrationTestScope Scope { get; set; }
    public List<string> TestMethods { get; set; } = new();
    public DateTime ScheduledStartTime { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public string TesterName { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class RemediationAction
{
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RemediationPriority Priority { get; set; }
    public TimeSpan EstimatedTime { get; set; }
    public List<string> Steps { get; set; } = new();
    public List<string> RequiredTools { get; set; } = new();
    public string? AutomationScript { get; set; }
}

public enum ScanType
{
    VulnerabilityAssessment,
    PenetrationTest,
    ComplianceScan,
    ConfigurationAudit,
    NetworkScan,
    WebApplicationScan,
    DatabaseScan
}

public enum ScanStatus
{
    Scheduled,
    Running,
    Completed,
    Failed,
    Cancelled,
    Paused
}

public enum VulnerabilitySeverity
{
    Info,
    Low,
    Medium,
    High,
    Critical
}

public enum VulnerabilityStatus
{
    Open,
    InProgress,
    Resolved,
    Accepted,
    FalsePositive
}

public enum RiskLevel
{
    VeryLow,
    Low,
    Medium,
    High,
    VeryHigh
}

public enum SecurityControlType
{
    Preventive,
    Detective,
    Corrective,
    Deterrent,
    Recovery,
    Compensating
}

public enum SecurityControlStatus
{
    Implemented,
    PartiallyImplemented,
    NotImplemented,
    UnderReview
}

public enum SecurityRecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum SecurityRecommendationStatus
{
    Pending,
    Approved,
    InProgress,
    Implemented,
    Rejected,
    Deferred
}

public enum SecurityPostureLevel
{
    Poor,
    Fair,
    Good,
    Excellent
}

public enum SecurityMetricStatus
{
    Normal,
    Warning,
    Critical
}

public enum SecurityAlertSeverity
{
    Info,
    Low,
    Medium,
    High,
    Critical
}

public enum SecurityAlertType
{
    VulnerabilityDetected,
    SecurityBreach,
    UnauthorizedAccess,
    SuspiciousActivity,
    ComplianceViolation,
    SystemAnomaly,
    ThreatDetected
}

public enum SecurityAlertStatus
{
    New,
    Acknowledged,
    InProgress,
    Resolved,
    Closed,
    FalsePositive
}

public enum PenetrationTestScope
{
    External,
    Internal,
    WebApplication,
    WirelessNetwork,
    SocialEngineering,
    PhysicalSecurity
}

public enum RemediationPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum ScanScope
{
    Network,
    WebApplication,
    Database,
    Infrastructure,
    CloudServices,
    MobileApplication
}
