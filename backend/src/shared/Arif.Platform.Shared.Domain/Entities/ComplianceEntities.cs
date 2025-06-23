using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class ComplianceReport : BaseEntity
{
    public Guid TenantId { get; set; }
    public ComplianceFramework Framework { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ComplianceStatus Status { get; set; }
    public double ComplianceScore { get; set; }
    public string ReportData { get; set; } = string.Empty;
    public List<ComplianceViolation> Violations { get; set; } = new();
    public List<ComplianceRecommendation> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
}

public class ComplianceViolation : BaseEntity
{
    public Guid TenantId { get; set; }
    public ComplianceFramework Framework { get; set; }
    public string RequirementId { get; set; } = string.Empty;
    public string RequirementName { get; set; } = string.Empty;
    public string ViolationDescription { get; set; } = string.Empty;
    public ComplianceSeverity Severity { get; set; }
    public ComplianceViolationStatus Status { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public Guid? AssignedTo { get; set; }
    public string Evidence { get; set; } = string.Empty;
}

public class ComplianceAssessment : BaseEntity
{
    public Guid TenantId { get; set; }
    public ComplianceFramework Framework { get; set; }
    public DateTime AssessmentDate { get; set; }
    public string AssessedBy { get; set; } = string.Empty;
    public double OverallScore { get; set; }
    public List<ComplianceControlAssessment> ControlAssessments { get; set; } = new();
    public List<ComplianceGap> IdentifiedGaps { get; set; } = new();
    public string ExecutiveSummary { get; set; } = string.Empty;
    public ComplianceAssessmentStatus Status { get; set; }
}

public class ComplianceControlAssessment : BaseEntity
{
    public string ControlId { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public string ControlDescription { get; set; } = string.Empty;
    public ComplianceControlStatus Status { get; set; }
    public double Score { get; set; }
    public string Evidence { get; set; } = string.Empty;
    public string AssessmentNotes { get; set; } = string.Empty;
    public List<string> RequiredActions { get; set; } = new();
}

public class ComplianceGap : BaseEntity
{
    public string GapDescription { get; set; } = string.Empty;
    public ComplianceSeverity Severity { get; set; }
    public string RequirementId { get; set; } = string.Empty;
    public List<string> RecommendedActions { get; set; } = new();
    public DateTime TargetResolutionDate { get; set; }
    public Guid? AssignedTo { get; set; }
}

public class ComplianceRecommendation : BaseEntity
{
    public Guid TenantId { get; set; }
    public ComplianceFramework Framework { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CompliancePriority Priority { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> ActionItems { get; set; } = new();
    public DateTime RecommendedBy { get; set; }
    public ComplianceRecommendationStatus Status { get; set; }
    public DateTime? ImplementedAt { get; set; }
    public string? ImplementationNotes { get; set; }
}

public class ComplianceRequirement : BaseEntity
{
    public ComplianceFramework Framework { get; set; }
    public string RequirementId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ComplianceSeverity Severity { get; set; }
    public List<string> ValidationCriteria { get; set; } = new();
    public List<string> EvidenceRequirements { get; set; } = new();
    public bool IsAutomated { get; set; }
    public string? AutomationScript { get; set; }
}

public class ComplianceMetrics : BaseEntity
{
    public Guid TenantId { get; set; }
    public ComplianceFramework Framework { get; set; }
    public DateTime MetricsDate { get; set; }
    public double OverallComplianceScore { get; set; }
    public int TotalRequirements { get; set; }
    public int CompliantRequirements { get; set; }
    public int NonCompliantRequirements { get; set; }
    public int PendingRequirements { get; set; }
    public Dictionary<string, double> CategoryScores { get; set; } = new();
    public List<ComplianceTrend> Trends { get; set; } = new();
}

public class ComplianceTrend
{
    public DateTime Date { get; set; }
    public double Score { get; set; }
    public string Category { get; set; } = string.Empty;
}

public enum ComplianceFramework
{
    SOC2,
    ISO27001,
    GDPR,
    HIPAA,
    PCI_DSS,
    NIST,
    CCPA
}

public enum ComplianceStatus
{
    Compliant,
    NonCompliant,
    PartiallyCompliant,
    UnderReview,
    NotApplicable
}

public enum ComplianceSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum ComplianceViolationStatus
{
    Open,
    InProgress,
    Resolved,
    Accepted,
    Deferred
}

public enum ComplianceAssessmentStatus
{
    Planned,
    InProgress,
    Completed,
    Approved,
    Rejected
}

public enum ComplianceControlStatus
{
    Implemented,
    PartiallyImplemented,
    NotImplemented,
    NotApplicable
}

public enum CompliancePriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum ComplianceRecommendationStatus
{
    Pending,
    Approved,
    InProgress,
    Implemented,
    Rejected,
    Deferred
}
