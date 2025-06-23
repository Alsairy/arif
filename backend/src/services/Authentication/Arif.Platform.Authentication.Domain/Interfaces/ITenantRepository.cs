using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Infrastructure.Repositories;

namespace Arif.Platform.Authentication.Domain.Interfaces;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, string subdomain, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
    Task<int> GetUserCountAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> CanAddUserAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> GetWithUsersAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
