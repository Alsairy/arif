using Microsoft.EntityFrameworkCore;
using Arif.Platform.ConfigurationManagement.Domain.Entities;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Infrastructure.Data;

namespace Arif.Platform.ConfigurationManagement.Infrastructure.Repositories;

public class ConfigurationDeploymentRepository : IConfigurationDeploymentRepository
{
    private readonly ConfigurationDbContext _context;

    public ConfigurationDeploymentRepository(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<ConfigurationDeployment?> GetByIdAsync(Guid id)
    {
        return await _context.ConfigurationDeployments
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<ConfigurationDeployment>> GetAllAsync(string environment, string application)
    {
        return await _context.ConfigurationDeployments
            .Include(d => d.Items)
            .Where(d => d.Environment == environment && d.Application == application)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<ConfigurationDeployment> CreateAsync(ConfigurationDeployment deployment)
    {
        deployment.Id = Guid.NewGuid();
        deployment.CreatedAt = DateTime.UtcNow;
        deployment.Status = ConfigurationDeploymentStatus.Pending;
        
        foreach (var item in deployment.Items)
        {
            item.Id = Guid.NewGuid();
            item.DeploymentId = deployment.Id;
            item.Status = ConfigurationDeploymentItemStatus.Pending;
        }
        
        _context.ConfigurationDeployments.Add(deployment);
        await _context.SaveChangesAsync();
        
        return deployment;
    }

    public async Task<ConfigurationDeployment> UpdateAsync(ConfigurationDeployment deployment)
    {
        _context.ConfigurationDeployments.Update(deployment);
        await _context.SaveChangesAsync();
        
        return deployment;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var deployment = await _context.ConfigurationDeployments.FindAsync(id);
        if (deployment == null)
            return false;

        _context.ConfigurationDeployments.Remove(deployment);
        await _context.SaveChangesAsync();
        return true;
    }
}
