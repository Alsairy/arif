using Arif.Platform.Authentication.Domain.DTOs;

namespace Arif.Platform.Authentication.Domain.Interfaces;

public interface ITenantManagementService
{
    Task<TenantResult> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken = default);
    Task<TenantResult> UpdateTenantAsync(UpdateTenantRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantInfo?> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenantInfo>> GetTenantsAsync(GetTenantsRequest request, CancellationToken cancellationToken = default);
    Task<bool> ActivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantStats> GetTenantStatsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> CanAddUserAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
