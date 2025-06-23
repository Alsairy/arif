using Microsoft.EntityFrameworkCore;
using Arif.Platform.ConfigurationManagement.Domain.Entities;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Domain.DTOs;
using Arif.Platform.ConfigurationManagement.Infrastructure.Data;

namespace Arif.Platform.ConfigurationManagement.Infrastructure.Repositories;

public class ConfigurationAuditRepository : IConfigurationAuditRepository
{
    private readonly ConfigurationDbContext _context;

    public ConfigurationAuditRepository(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<ConfigurationAuditLog> CreateAsync(ConfigurationAuditLog auditLog)
    {
        auditLog.Id = Guid.NewGuid();
        auditLog.Timestamp = DateTime.UtcNow;
        
        _context.ConfigurationAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
        
        return auditLog;
    }

    public async Task<List<ConfigurationAuditLog>> GetByFilterAsync(AuditLogFilter filter)
    {
        var query = _context.ConfigurationAuditLogs.AsQueryable();

        if (filter.ConfigurationId.HasValue)
            query = query.Where(a => a.ConfigurationId == filter.ConfigurationId.Value);

        if (filter.FeatureFlagId.HasValue)
            query = query.Where(a => a.FeatureFlagId == filter.FeatureFlagId.Value);

        if (filter.DeploymentId.HasValue)
            query = query.Where(a => a.DeploymentId == filter.DeploymentId.Value);

        if (!string.IsNullOrEmpty(filter.Action))
            query = query.Where(a => a.Action == filter.Action);

        if (!string.IsNullOrEmpty(filter.EntityType))
            query = query.Where(a => a.EntityType == filter.EntityType);

        if (!string.IsNullOrEmpty(filter.UserId))
            query = query.Where(a => a.UserId == filter.UserId);

        if (filter.StartDate.HasValue)
            query = query.Where(a => a.Timestamp >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(a => a.Timestamp <= filter.EndDate.Value);

        if (filter.TenantId.HasValue)
            query = query.Where(a => a.TenantId == filter.TenantId.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
    }

    public async Task<List<ConfigurationAuditLog>> GetByConfigurationIdAsync(Guid configurationId)
    {
        return await _context.ConfigurationAuditLogs
            .Where(a => a.ConfigurationId == configurationId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<List<ConfigurationAuditLog>> GetByFeatureFlagIdAsync(Guid featureFlagId)
    {
        return await _context.ConfigurationAuditLogs
            .Where(a => a.FeatureFlagId == featureFlagId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<List<ConfigurationAuditLog>> GetByDeploymentIdAsync(Guid deploymentId)
    {
        return await _context.ConfigurationAuditLogs
            .Where(a => a.DeploymentId == deploymentId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}
