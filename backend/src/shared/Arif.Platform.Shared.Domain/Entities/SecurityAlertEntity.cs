namespace Arif.Platform.Shared.Domain.Entities;

public class SecurityAlertEntity : BaseEntity
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string? IpAddress { get; set; }
    public string? Details { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }

    public User? User { get; set; }
    public Tenant? Tenant { get; set; }
}
