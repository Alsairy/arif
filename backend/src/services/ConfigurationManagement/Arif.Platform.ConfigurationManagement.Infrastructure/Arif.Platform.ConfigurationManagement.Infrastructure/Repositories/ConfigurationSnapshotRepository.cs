using Microsoft.EntityFrameworkCore;
using Arif.Platform.ConfigurationManagement.Domain.Entities;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Infrastructure.Data;

namespace Arif.Platform.ConfigurationManagement.Infrastructure.Repositories;

public class ConfigurationSnapshotRepository : IConfigurationSnapshotRepository
{
    private readonly ConfigurationDbContext _context;

    public ConfigurationSnapshotRepository(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<ConfigurationSnapshot?> GetByIdAsync(Guid id)
    {
        return await _context.ConfigurationSnapshots.FindAsync(id);
    }

    public async Task<List<ConfigurationSnapshot>> GetAllAsync(string environment, string application)
    {
        return await _context.ConfigurationSnapshots
            .Where(s => s.Environment == environment && s.Application == application)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<ConfigurationSnapshot> CreateAsync(ConfigurationSnapshot snapshot)
    {
        _context.ConfigurationSnapshots.Add(snapshot);
        await _context.SaveChangesAsync();
        
        return snapshot;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var snapshot = await _context.ConfigurationSnapshots.FindAsync(id);
        if (snapshot == null)
            return false;

        _context.ConfigurationSnapshots.Remove(snapshot);
        await _context.SaveChangesAsync();
        return true;
    }
}
