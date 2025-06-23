using Microsoft.EntityFrameworkCore;
using Arif.Platform.ConfigurationManagement.Domain.Entities;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Infrastructure.Data;

namespace Arif.Platform.ConfigurationManagement.Infrastructure.Repositories;

public class FeatureFlagRepository : IFeatureFlagRepository
{
    private readonly ConfigurationDbContext _context;

    public FeatureFlagRepository(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<FeatureFlag?> GetByNameAsync(string name, string environment, string application, Guid? tenantId = null)
    {
        return await _context.FeatureFlags
            .Include(f => f.Rules)
            .Include(f => f.Schedule)
            .FirstOrDefaultAsync(f => f.Name == name && 
                                    f.Environment == environment && 
                                    f.Application == application && 
                                    f.TenantId == tenantId);
    }

    public async Task<FeatureFlag?> GetByIdAsync(Guid id)
    {
        return await _context.FeatureFlags
            .Include(f => f.Rules)
            .Include(f => f.Schedule)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<List<FeatureFlag>> GetAllAsync(string environment, string application, Guid? tenantId = null)
    {
        return await _context.FeatureFlags
            .Include(f => f.Rules)
            .Include(f => f.Schedule)
            .Where(f => f.Environment == environment && 
                       f.Application == application && 
                       f.TenantId == tenantId)
            .OrderBy(f => f.Name)
            .ToListAsync();
    }

    public async Task<FeatureFlag> CreateAsync(FeatureFlag featureFlag)
    {
        featureFlag.Id = Guid.NewGuid();
        featureFlag.CreatedAt = DateTime.UtcNow;
        featureFlag.UpdatedAt = DateTime.UtcNow;
        
        foreach (var rule in featureFlag.Rules)
        {
            rule.Id = Guid.NewGuid();
            rule.FeatureFlagId = featureFlag.Id;
        }
        
        if (featureFlag.Schedule != null)
        {
            featureFlag.Schedule.Id = Guid.NewGuid();
            featureFlag.Schedule.FeatureFlagId = featureFlag.Id;
        }
        
        _context.FeatureFlags.Add(featureFlag);
        await _context.SaveChangesAsync();
        
        return featureFlag;
    }

    public async Task<FeatureFlag> UpdateAsync(FeatureFlag featureFlag)
    {
        featureFlag.UpdatedAt = DateTime.UtcNow;
        
        _context.FeatureFlags.Update(featureFlag);
        await _context.SaveChangesAsync();
        
        return featureFlag;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var featureFlag = await _context.FeatureFlags
            .Include(f => f.Rules)
            .Include(f => f.Schedule)
            .FirstOrDefaultAsync(f => f.Id == id);
            
        if (featureFlag == null)
            return false;

        _context.FeatureFlags.Remove(featureFlag);
        await _context.SaveChangesAsync();
        return true;
    }
}
