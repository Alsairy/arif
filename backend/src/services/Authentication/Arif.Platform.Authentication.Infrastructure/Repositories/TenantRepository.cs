using Microsoft.EntityFrameworkCore;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Infrastructure.Data;
using Arif.Platform.Shared.Infrastructure.Repositories;
using Arif.Platform.Authentication.Domain.Interfaces;

namespace Arif.Platform.Authentication.Infrastructure.Repositories;

public class TenantRepository : BaseRepository<Tenant>, ITenantRepository
{
    public TenantRepository(ArifPlatformDbContext context) : base(context)
    {
    }

    public async Task<Tenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Subdomain == subdomain, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string name, string subdomain, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(t => t.Name == name || t.Subdomain == subdomain, cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUserCountAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .CountAsync(u => u.TenantId == tenantId && !u.IsDeleted, cancellationToken);
    }

    public async Task<bool> CanAddUserAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) return false;

        var currentUserCount = await GetUserCountAsync(tenantId, cancellationToken);
        return currentUserCount < tenant.MaxUsers;
    }

    public async Task<Tenant?> GetWithUsersAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Users.Where(u => !u.IsDeleted))
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
    }
}
