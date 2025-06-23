using System.Security.Claims;

namespace Arif.Platform.Shared.Common.Security;

public interface IAuditLogger
{
    Task LogAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
    Task LogUserActionAsync(string action, Guid userId, Guid tenantId, object? details = null, CancellationToken cancellationToken = default);
    Task LogSecurityEventAsync(SecurityEventType eventType, string description, Guid? userId = null, Guid? tenantId = null, object? details = null, CancellationToken cancellationToken = default);
    Task LogDataAccessAsync(string resource, DataAccessType accessType, Guid userId, Guid tenantId, object? details = null, CancellationToken cancellationToken = default);
}

public class AuditEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Resource { get; set; }
    public string Description { get; set; } = string.Empty;
    public object? Details { get; set; }
    public AuditSeverity Severity { get; set; } = AuditSeverity.Information;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

public enum SecurityEventType
{
    LoginAttempt,
    LoginSuccess,
    LoginFailure,
    Logout,
    PasswordChange,
    PasswordReset,
    TokenRefresh,
    UnauthorizedAccess,
    SuspiciousActivity,
    DataBreach,
    ConfigurationChange,
    PermissionChange,
    AccountLockout,
    TwoFactorEnabled,
    TwoFactorDisabled
}

public enum DataAccessType
{
    Read,
    Create,
    Update,
    Delete,
    Export,
    Import,
    Share
}

public enum AuditSeverity
{
    Information,
    Warning,
    Error,
    Critical
}
