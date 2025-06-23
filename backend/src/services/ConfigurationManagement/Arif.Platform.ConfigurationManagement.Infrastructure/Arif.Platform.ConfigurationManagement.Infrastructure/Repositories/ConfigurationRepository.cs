using Microsoft.EntityFrameworkCore;
using Arif.Platform.ConfigurationManagement.Domain.Entities;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Infrastructure.Data;

namespace Arif.Platform.ConfigurationManagement.Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly ConfigurationDbContext _context;

    public ConfigurationRepository(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<Configuration?> GetByKeyAsync(string key, string environment, string application, Guid? tenantId = null)
    {
        return await _context.Configurations
            .Include(c => c.ValidationRule)
            .FirstOrDefaultAsync(c => c.Key == key && 
                                    c.Environment == environment && 
                                    c.Application == application && 
                                    c.TenantId == tenantId &&
                                    c.IsActive);
    }

    public async Task<Configuration?> GetByIdAsync(Guid id)
    {
        return await _context.Configurations
            .Include(c => c.ValidationRule)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Configuration>> GetAllAsync(string environment, string application, Guid? tenantId = null)
    {
        return await _context.Configurations
            .Include(c => c.ValidationRule)
            .Where(c => c.Environment == environment && 
                       c.Application == application && 
                       c.TenantId == tenantId &&
                       c.IsActive)
            .OrderBy(c => c.Key)
            .ToListAsync();
    }

    public async Task<Configuration> CreateAsync(Configuration configuration)
    {
        configuration.Id = Guid.NewGuid();
        configuration.CreatedAt = DateTime.UtcNow;
        configuration.UpdatedAt = DateTime.UtcNow;
        
        _context.Configurations.Add(configuration);
        await _context.SaveChangesAsync();
        
        return configuration;
    }

    public async Task<Configuration> UpdateAsync(Configuration configuration)
    {
        configuration.UpdatedAt = DateTime.UtcNow;
        configuration.Version++;
        
        _context.Configurations.Update(configuration);
        await _context.SaveChangesAsync();
        
        return configuration;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var configuration = await _context.Configurations.FindAsync(id);
        if (configuration == null)
            return false;

        configuration.IsActive = false;
        configuration.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Configuration>> GetByTagsAsync(List<string> tags, string environment, string application)
    {
        return await _context.Configurations
            .Include(c => c.ValidationRule)
            .Where(c => c.Environment == environment && 
                       c.Application == application && 
                       c.IsActive &&
                       tags.Any(tag => c.Tags.Contains(tag)))
            .OrderBy(c => c.Key)
            .ToListAsync();
    }
}
