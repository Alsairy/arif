namespace Arif.Platform.Shared.Infrastructure.Services;

public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantSubdomain { get; }
    bool IsSystemContext { get; }
    void SetTenantId(Guid tenantId);
    void SetTenantSubdomain(string subdomain);
}
