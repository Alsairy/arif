namespace Arif.Platform.Shared.Domain.Entities;

public interface ITenantAware
{
    Guid TenantId { get; set; }
}
