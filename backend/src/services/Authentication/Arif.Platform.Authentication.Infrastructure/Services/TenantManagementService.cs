using Microsoft.Extensions.Logging;
using Arif.Platform.Authentication.Domain.Interfaces;
using Arif.Platform.Authentication.Domain.DTOs;
using Arif.Platform.Shared.Domain.Entities;

namespace Arif.Platform.Authentication.Infrastructure.Services;

public class TenantManagementService : ITenantManagementService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TenantManagementService> _logger;

    public TenantManagementService(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        ILogger<TenantManagementService> logger)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<TenantResult> CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _tenantRepository.ExistsAsync(request.Name, request.Subdomain, cancellationToken))
            {
                return new TenantResult(false, ErrorMessage: "Tenant with this name or subdomain already exists");
            }

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Subdomain = request.Subdomain,
                ContactEmail = request.ContactEmail,
                Description = request.Description,
                Language = request.Language,
                TimeZone = request.TimeZone,
                MaxUsers = request.MaxUsers,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _tenantRepository.AddAsync(tenant, cancellationToken);
            await _tenantRepository.SaveChangesAsync(cancellationToken);

            var tenantInfo = new TenantInfo(
                tenant.Id,
                tenant.Name,
                tenant.Subdomain,
                tenant.ContactEmail,
                tenant.Description,
                tenant.IsActive,
                tenant.CreatedAt,
                tenant.Language,
                tenant.TimeZone,
                tenant.MaxUsers,
                0
            );

            _logger.LogInformation("Tenant created successfully: {TenantId}", tenant.Id);
            return new TenantResult(true, tenantInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant with name: {Name}", request.Name);
            return new TenantResult(false, ErrorMessage: "An error occurred while creating the tenant");
        }
    }

    public async Task<TenantResult> UpdateTenantAsync(UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
            if (tenant == null)
            {
                return new TenantResult(false, ErrorMessage: "Tenant not found");
            }

            if (!string.IsNullOrEmpty(request.Name) && request.Name != tenant.Name)
            {
                if (await _tenantRepository.ExistsAsync(request.Name, "", cancellationToken))
                {
                    return new TenantResult(false, ErrorMessage: "Tenant name already exists");
                }
                tenant.Name = request.Name;
            }

            if (!string.IsNullOrEmpty(request.Subdomain) && request.Subdomain != tenant.Subdomain)
            {
                if (await _tenantRepository.ExistsAsync("", request.Subdomain, cancellationToken))
                {
                    return new TenantResult(false, ErrorMessage: "Subdomain already exists");
                }
                tenant.Subdomain = request.Subdomain;
            }

            if (!string.IsNullOrEmpty(request.ContactEmail))
                tenant.ContactEmail = request.ContactEmail;

            if (!string.IsNullOrEmpty(request.Description))
                tenant.Description = request.Description;

            if (!string.IsNullOrEmpty(request.Language))
                tenant.Language = request.Language;

            if (!string.IsNullOrEmpty(request.TimeZone))
                tenant.TimeZone = request.TimeZone;

            if (request.MaxUsers.HasValue)
                tenant.MaxUsers = request.MaxUsers.Value;

            if (request.IsActive.HasValue)
                tenant.IsActive = request.IsActive.Value;

            tenant.UpdatedAt = DateTime.UtcNow;

            await _tenantRepository.UpdateAsync(tenant, cancellationToken);
            await _tenantRepository.SaveChangesAsync(cancellationToken);

            var currentUserCount = await _tenantRepository.GetUserCountAsync(tenant.Id, cancellationToken);

            var tenantInfo = new TenantInfo(
                tenant.Id,
                tenant.Name,
                tenant.Subdomain,
                tenant.ContactEmail,
                tenant.Description,
                tenant.IsActive,
                tenant.CreatedAt,
                tenant.Language,
                tenant.TimeZone,
                tenant.MaxUsers,
                currentUserCount
            );

            _logger.LogInformation("Tenant updated successfully: {TenantId}", tenant.Id);
            return new TenantResult(true, tenantInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant: {TenantId}", request.TenantId);
            return new TenantResult(false, ErrorMessage: "An error occurred while updating the tenant");
        }
    }

    public async Task<bool> DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null) return false;

            await _tenantRepository.DeleteAsync(tenant, cancellationToken);
            await _tenantRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tenant deleted successfully: {TenantId}", tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant: {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<TenantInfo?> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null) return null;

            var currentUserCount = await _tenantRepository.GetUserCountAsync(tenant.Id, cancellationToken);

            return new TenantInfo(
                tenant.Id,
                tenant.Name,
                tenant.Subdomain,
                tenant.ContactEmail,
                tenant.Description,
                tenant.IsActive,
                tenant.CreatedAt,
                tenant.Language,
                tenant.TimeZone,
                tenant.MaxUsers,
                currentUserCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant: {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<IEnumerable<TenantInfo>> GetTenantsAsync(GetTenantsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenants = await _tenantRepository.GetAllAsync(cancellationToken);

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                tenants = tenants.Where(t => 
                    t.Name.ToLower().Contains(searchTerm) ||
                    t.Subdomain.ToLower().Contains(searchTerm) ||
                    t.ContactEmail.ToLower().Contains(searchTerm));
            }

            if (request.IsActive.HasValue)
            {
                tenants = tenants.Where(t => t.IsActive == request.IsActive.Value);
            }

            var tenantsList = tenants.ToList();
            var skip = (request.Page - 1) * request.PageSize;
            var pagedTenants = tenantsList.Skip(skip).Take(request.PageSize);

            var tenantInfos = new List<TenantInfo>();
            foreach (var tenant in pagedTenants)
            {
                var currentUserCount = await _tenantRepository.GetUserCountAsync(tenant.Id, cancellationToken);

                tenantInfos.Add(new TenantInfo(
                    tenant.Id,
                    tenant.Name,
                    tenant.Subdomain,
                    tenant.ContactEmail,
                    tenant.Description,
                    tenant.IsActive,
                    tenant.CreatedAt,
                    tenant.Language,
                    tenant.TimeZone,
                    tenant.MaxUsers,
                    currentUserCount
                ));
            }

            return tenantInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenants");
            return Enumerable.Empty<TenantInfo>();
        }
    }

    public async Task<bool> ActivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null) return false;

            tenant.IsActive = true;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _tenantRepository.UpdateAsync(tenant, cancellationToken);
            await _tenantRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tenant activated: {TenantId}", tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating tenant: {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> DeactivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null) return false;

            tenant.IsActive = false;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _tenantRepository.UpdateAsync(tenant, cancellationToken);
            await _tenantRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tenant deactivated: {TenantId}", tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating tenant: {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<TenantStats> GetTenantStatsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null)
            {
                return new TenantStats(tenantId, "", 0, 0, 0, 0, 0, 0, 0, DateTime.MinValue);
            }

            var users = await _userRepository.GetUsersByTenantAsync(tenantId, cancellationToken);
            var usersList = users.ToList();

            var totalUsers = usersList.Count;
            var activeUsers = usersList.Count(u => u.IsActive);
            var inactiveUsers = totalUsers - activeUsers;

            var lastActivity = usersList
                .Where(u => u.LastLoginAt.HasValue)
                .Max(u => u.LastLoginAt) ?? DateTime.MinValue;

            return new TenantStats(
                tenantId,
                tenant.Name,
                totalUsers,
                activeUsers,
                inactiveUsers,
                0, // TotalChatbots - would need chatbot repository
                0, // ActiveChatbots - would need chatbot repository
                0, // TotalSessions - would need session repository
                0, // TotalMessages - would need message repository
                lastActivity
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant stats: {TenantId}", tenantId);
            return new TenantStats(tenantId, "", 0, 0, 0, 0, 0, 0, 0, DateTime.MinValue);
        }
    }

    public async Task<bool> CanAddUserAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _tenantRepository.CanAddUserAsync(tenantId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if tenant can add user: {TenantId}", tenantId);
            return false;
        }
    }
}
