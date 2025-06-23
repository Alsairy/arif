using Arif.Platform.Authentication.Domain.DTOs;

namespace Arif.Platform.Authentication.Domain.Interfaces;

public interface IUserManagementService
{
    Task<UserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResult> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserInfo>> GetUsersAsync(GetUsersRequest request, CancellationToken cancellationToken = default);
    Task<bool> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default);
    Task<bool> RemoveRoleAsync(RemoveRoleRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
