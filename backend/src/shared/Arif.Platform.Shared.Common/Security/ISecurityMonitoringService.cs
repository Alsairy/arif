namespace Arif.Platform.Shared.Common.Security;

public interface ISecurityMonitoringService
{
    Task MonitorSuspiciousActivityAsync(string activity, Guid? userId = null, Guid? tenantId = null, object? details = null, CancellationToken cancellationToken = default);
    Task MonitorFailedLoginAttemptsAsync(string email, string ipAddress, CancellationToken cancellationToken = default);
    Task MonitorUnauthorizedAccessAsync(string resource, Guid? userId = null, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task MonitorDataAccessPatternsAsync(Guid userId, string resource, DataAccessType accessType, CancellationToken cancellationToken = default);
    Task CheckForAnomalousActivityAsync(Guid userId, CancellationToken cancellationToken = default);
    Task GenerateSecurityReportAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}

public class SecurityAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public SecurityAlertType AlertType { get; set; }
    public SecurityAlertSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string? IpAddress { get; set; }
    public object? Details { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
}

public enum SecurityAlertType
{
    SuspiciousLogin,
    MultipleFailedLogins,
    UnauthorizedAccess,
    DataBreach,
    AnomalousActivity,
    PrivilegeEscalation,
    MaliciousRequest,
    AccountTakeover
}

public enum SecurityAlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}
