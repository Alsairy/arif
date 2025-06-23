using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Infrastructure.Repositories;

namespace Arif.Platform.Authentication.Domain.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
    Task AddPermissionToRoleAsync(Guid permissionId, Guid roleId, CancellationToken cancellationToken = default);
    Task RemovePermissionFromRoleAsync(Guid permissionId, Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> RoleHasPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetSystemPermissionsAsync(CancellationToken cancellationToken = default);
}
