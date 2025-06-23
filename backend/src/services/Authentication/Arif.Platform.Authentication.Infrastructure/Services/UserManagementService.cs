using Microsoft.Extensions.Logging;
using Arif.Platform.Shared.Common.Authentication;
using Arif.Platform.Authentication.Domain.Interfaces;
using Arif.Platform.Authentication.Domain.DTOs;
using Arif.Platform.Shared.Domain.Entities;

namespace Arif.Platform.Authentication.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserManagementService> logger)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
            if (tenant == null)
            {
                return new UserResult(false, ErrorMessage: "Tenant not found");
            }

            if (!await _tenantRepository.CanAddUserAsync(request.TenantId, cancellationToken))
            {
                return new UserResult(false, ErrorMessage: "Tenant has reached maximum user limit");
            }

            if (await _userRepository.ExistsAsync(request.Email, request.Username ?? "", cancellationToken))
            {
                return new UserResult(false, ErrorMessage: "User with this email or username already exists");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                TenantId = request.TenantId,
                Language = request.Language,
                TimeZone = request.TimeZone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            if (request.Roles != null && request.Roles.Any())
            {
                foreach (var roleName in request.Roles)
                {
                    var role = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
                    if (role != null && role.TenantId == request.TenantId)
                    {
                        await _roleRepository.AddUserToRoleAsync(user.Id, role.Id, cancellationToken);
                    }
                }
                await _roleRepository.SaveChangesAsync(cancellationToken);
            }

            var userInfo = new UserInfo(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Username,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt,
                user.Language,
                user.TimeZone,
                user.TenantId,
                tenant.Name,
                request.Roles ?? Enumerable.Empty<string>(),
                Enumerable.Empty<string>()
            );

            _logger.LogInformation("User created successfully: {UserId}", user.Id);
            return new UserResult(true, userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email: {Email}", request.Email);
            return new UserResult(false, ErrorMessage: "An error occurred while creating the user");
        }
    }

    public async Task<UserResult> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return new UserResult(false, ErrorMessage: "User not found");
            }

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                if (await _userRepository.ExistsAsync(request.Email, "", cancellationToken))
                {
                    return new UserResult(false, ErrorMessage: "Email already exists");
                }
                user.Email = request.Email;
            }

            if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
            {
                if (await _userRepository.ExistsAsync("", request.Username, cancellationToken))
                {
                    return new UserResult(false, ErrorMessage: "Username already exists");
                }
                user.Username = request.Username;
            }

            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.Language))
                user.Language = request.Language;

            if (!string.IsNullOrEmpty(request.TimeZone))
                user.TimeZone = request.TimeZone;

            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
            var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

            var userInfo = new UserInfo(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Username,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt,
                user.Language,
                user.TimeZone,
                user.TenantId,
                user.Tenant?.Name ?? "",
                roles,
                permissions
            );

            _logger.LogInformation("User updated successfully: {UserId}", user.Id);
            return new UserResult(true, userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", request.UserId);
            return new UserResult(false, ErrorMessage: "An error occurred while updating the user");
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;

            await _userRepository.DeleteAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User deleted successfully: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return false;
        }
    }

    public async Task<UserInfo?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetWithRolesAsync(userId, cancellationToken);
            if (user == null) return null;

            var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
            var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

            return new UserInfo(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Username,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt,
                user.Language,
                user.TimeZone,
                user.TenantId,
                user.Tenant?.Name ?? "",
                roles,
                permissions
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", userId);
            return null;
        }
    }

    public async Task<IEnumerable<UserInfo>> GetUsersAsync(GetUsersRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);

            if (request.TenantId.HasValue)
            {
                users = users.Where(u => u.TenantId == request.TenantId.Value);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                users = users.Where(u => 
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm) ||
                    (u.Username?.ToLower().Contains(searchTerm) ?? false));
            }

            if (request.IsActive.HasValue)
            {
                users = users.Where(u => u.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(request.Role))
            {
                var usersWithRole = await _userRepository.GetUsersByRoleAsync(request.Role, cancellationToken);
                var userIds = usersWithRole.Select(u => u.Id).ToHashSet();
                users = users.Where(u => userIds.Contains(u.Id));
            }

            var usersList = users.ToList();
            var skip = (request.Page - 1) * request.PageSize;
            var pagedUsers = usersList.Skip(skip).Take(request.PageSize);

            var userInfos = new List<UserInfo>();
            foreach (var user in pagedUsers)
            {
                var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
                var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

                userInfos.Add(new UserInfo(
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.Username,
                    user.IsActive,
                    user.CreatedAt,
                    user.LastLoginAt,
                    user.Language,
                    user.TimeZone,
                    user.TenantId,
                    user.Tenant?.Name ?? "",
                    roles,
                    permissions
                ));
            }

            return userInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return Enumerable.Empty<UserInfo>();
        }
    }

    public async Task<bool> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null) return false;

            var role = await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken);
            if (role == null || role.TenantId != user.TenantId) return false;

            if (await _roleRepository.IsUserInRoleAsync(request.UserId, role.Id, cancellationToken))
            {
                return true;
            }

            await _roleRepository.AddUserToRoleAsync(request.UserId, role.Id, cancellationToken);
            await _roleRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role assigned to user: {UserId}, Role: {RoleName}", request.UserId, request.RoleName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user: {UserId}, Role: {RoleName}", request.UserId, request.RoleName);
            return false;
        }
    }

    public async Task<bool> RemoveRoleAsync(RemoveRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null) return false;

            var role = await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken);
            if (role == null || role.TenantId != user.TenantId) return false;

            await _roleRepository.RemoveUserFromRoleAsync(request.UserId, role.Id, cancellationToken);
            await _roleRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role removed from user: {UserId}, Role: {RoleName}", request.UserId, request.RoleName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user: {UserId}, Role: {RoleName}", request.UserId, request.RoleName);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _userRepository.GetUserRolesAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles: {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _userRepository.GetUserPermissionsAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user permissions: {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User activated: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User deactivated: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
            return false;
        }
    }
}
