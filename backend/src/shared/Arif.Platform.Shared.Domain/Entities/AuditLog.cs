namespace Arif.Platform.Shared.Domain.Entities;

public class AuditLog : BaseEntity
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Resource { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string Severity { get; set; } = string.Empty;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }

    public User? User { get; set; }
    public Tenant? Tenant { get; set; }
}
