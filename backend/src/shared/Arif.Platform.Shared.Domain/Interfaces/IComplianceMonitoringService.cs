using Arif.Platform.Shared.Domain.Entities;

namespace Arif.Platform.Shared.Domain.Interfaces;

public interface IComplianceMonitoringService
{
    Task<ComplianceReport> GenerateSOC2ReportAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<ComplianceReport> GenerateISO27001ReportAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<ComplianceReport> GenerateGDPRReportAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<ComplianceStatus> GetComplianceStatusAsync(Guid tenantId, ComplianceFramework framework, CancellationToken cancellationToken = default);
    Task<List<ComplianceViolation>> GetComplianceViolationsAsync(Guid tenantId, ComplianceFramework framework, CancellationToken cancellationToken = default);
    Task<bool> ValidateComplianceRequirementAsync(Guid tenantId, ComplianceRequirement requirement, CancellationToken cancellationToken = default);
    Task<ComplianceAssessment> PerformComplianceAssessmentAsync(Guid tenantId, ComplianceFramework framework, CancellationToken cancellationToken = default);
    Task<List<ComplianceRecommendation>> GetComplianceRecommendationsAsync(Guid tenantId, ComplianceFramework framework, CancellationToken cancellationToken = default);
    Task<bool> ScheduleComplianceAuditAsync(Guid tenantId, ComplianceFramework framework, DateTime scheduledDate, CancellationToken cancellationToken = default);
    Task<ComplianceMetrics> GetComplianceMetricsAsync(Guid tenantId, ComplianceFramework framework, CancellationToken cancellationToken = default);
}

public interface ISecurityScanningService
{
    Task<SecurityScanResult> PerformVulnerabilityAssessmentAsync(Guid tenantId, ScanScope scope, CancellationToken cancellationToken = default);
    Task<SecurityScanResult> PerformPenetrationTestAsync(Guid tenantId, PenetrationTestConfig config, CancellationToken cancellationToken = default);
    Task<List<SecurityVulnerability>> GetActiveVulnerabilitiesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<SecurityRiskAssessment> PerformRiskAssessmentAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> ScheduleSecurityScanAsync(Guid tenantId, ScanType scanType, DateTime scheduledDate, CancellationToken cancellationToken = default);
    Task<SecurityScanHistory> GetScanHistoryAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<SecurityRecommendation>> GetSecurityRecommendationsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> RemediateVulnerabilityAsync(Guid vulnerabilityId, RemediationAction action, CancellationToken cancellationToken = default);
    Task<SecurityPosture> GetSecurityPostureAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<SecurityAlert>> GetSecurityAlertsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IDataLossPreventionService
{
    Task<DLPScanResult> ScanContentAsync(string content, Guid tenantId, DLPPolicy policy, CancellationToken cancellationToken = default);
    Task<List<DLPViolation>> GetDLPViolationsAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<bool> CreateDLPPolicyAsync(Guid tenantId, DLPPolicy policy, CancellationToken cancellationToken = default);
    Task<bool> UpdateDLPPolicyAsync(Guid policyId, DLPPolicy policy, CancellationToken cancellationToken = default);
    Task<List<DLPPolicy>> GetDLPPoliciesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<DLPMetrics> GetDLPMetricsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> QuarantineContentAsync(Guid contentId, string reason, CancellationToken cancellationToken = default);
    Task<bool> ReleaseQuarantinedContentAsync(Guid contentId, string approver, CancellationToken cancellationToken = default);
    Task<List<QuarantinedContent>> GetQuarantinedContentAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<DLPIncidentResponse> HandleDLPIncidentAsync(Guid incidentId, DLPIncidentAction action, CancellationToken cancellationToken = default);
}

public interface IAuditTrailService
{
    Task<List<AuditTrailEntry>> GetAuditTrailAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<AuditTrailEntry> CreateAuditEntryAsync(AuditTrailEntry entry, CancellationToken cancellationToken = default);
    Task<List<AuditTrailEntry>> SearchAuditTrailAsync(Guid tenantId, AuditSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<AuditTrailReport> GenerateAuditReportAsync(Guid tenantId, DateTime startDate, DateTime endDate, AuditReportType reportType, CancellationToken cancellationToken = default);
    Task<bool> ArchiveAuditTrailAsync(Guid tenantId, DateTime beforeDate, CancellationToken cancellationToken = default);
    Task<AuditTrailMetrics> GetAuditMetricsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<AuditTrailEntry>> GetUserActivityAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<AuditTrailEntry>> GetSystemEventsAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<bool> ValidateAuditIntegrityAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<AuditTrailExport> ExportAuditTrailAsync(Guid tenantId, DateTime startDate, DateTime endDate, ExportFormat format, CancellationToken cancellationToken = default);
}
