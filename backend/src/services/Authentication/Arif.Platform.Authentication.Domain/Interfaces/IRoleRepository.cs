using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Infrastructure.Repositories;

namespace Arif.Platform.Authentication.Domain.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Role?> GetWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, Guid tenantId, CancellationToken cancellationToken = default);
    Task AddUserToRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task RemoveUserFromRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> IsUserInRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}
