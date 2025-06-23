using Microsoft.Extensions.Logging;
using Arif.Platform.ConfigurationManagement.Domain.Entities;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Domain.DTOs;

namespace Arif.Platform.ConfigurationManagement.Infrastructure.Services;

public class ConfigurationAuditService : IConfigurationAuditService
{
    private readonly IConfigurationAuditRepository _auditRepository;
    private readonly ILogger<ConfigurationAuditService> _logger;

    public ConfigurationAuditService(
        IConfigurationAuditRepository auditRepository,
        ILogger<ConfigurationAuditService> logger)
    {
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task LogConfigurationChangeAsync(Guid configurationId, string action, string? oldValue, string? newValue, string userId)
    {
        var auditLog = new ConfigurationAuditLog
        {
            ConfigurationId = configurationId,
            Action = action,
            EntityType = "Configuration",
            OldValue = oldValue,
            NewValue = newValue,
            UserId = userId,
            UserName = userId,
            IpAddress = "127.0.0.1",
            UserAgent = "System",
            AdditionalData = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["source"] = "ConfigurationService"
            }
        };

        await _auditRepository.CreateAsync(auditLog);
        
        _logger.LogInformation("Configuration audit logged: {Action} for {ConfigurationId} by {UserId}", 
            action, configurationId, userId);
    }

    public async Task LogFeatureFlagChangeAsync(Guid featureFlagId, string action, string? oldValue, string? newValue, string userId)
    {
        var auditLog = new ConfigurationAuditLog
        {
            FeatureFlagId = featureFlagId,
            Action = action,
            EntityType = "FeatureFlag",
            OldValue = oldValue,
            NewValue = newValue,
            UserId = userId,
            UserName = userId,
            IpAddress = "127.0.0.1",
            UserAgent = "System",
            AdditionalData = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["source"] = "FeatureFlagService"
            }
        };

        await _auditRepository.CreateAsync(auditLog);
        
        _logger.LogInformation("Feature flag audit logged: {Action} for {FeatureFlagId} by {UserId}", 
            action, featureFlagId, userId);
    }

    public async Task LogDeploymentActionAsync(Guid deploymentId, string action, string userId)
    {
        var auditLog = new ConfigurationAuditLog
        {
            DeploymentId = deploymentId,
            Action = action,
            EntityType = "Deployment",
            UserId = userId,
            UserName = userId,
            IpAddress = "127.0.0.1",
            UserAgent = "System",
            AdditionalData = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["source"] = "DeploymentService"
            }
        };

        await _auditRepository.CreateAsync(auditLog);
        
        _logger.LogInformation("Deployment audit logged: {Action} for {DeploymentId} by {UserId}", 
            action, deploymentId, userId);
    }

    public async Task<List<ConfigurationAuditLogDto>> GetAuditLogsAsync(AuditLogFilter filter)
    {
        var auditLogs = await _auditRepository.GetByFilterAsync(filter);
        return auditLogs.Select(MapToDto).ToList();
    }

    public async Task<List<ConfigurationAuditLogDto>> GetConfigurationAuditLogsAsync(Guid configurationId)
    {
        var auditLogs = await _auditRepository.GetByConfigurationIdAsync(configurationId);
        return auditLogs.Select(MapToDto).ToList();
    }

    public async Task<List<ConfigurationAuditLogDto>> GetFeatureFlagAuditLogsAsync(Guid featureFlagId)
    {
        var auditLogs = await _auditRepository.GetByFeatureFlagIdAsync(featureFlagId);
        return auditLogs.Select(MapToDto).ToList();
    }

    public async Task<List<ConfigurationAuditLogDto>> GetDeploymentAuditLogsAsync(Guid deploymentId)
    {
        var auditLogs = await _auditRepository.GetByDeploymentIdAsync(deploymentId);
        return auditLogs.Select(MapToDto).ToList();
    }

    private static ConfigurationAuditLogDto MapToDto(ConfigurationAuditLog auditLog)
    {
        return new ConfigurationAuditLogDto
        {
            Id = auditLog.Id,
            ConfigurationId = auditLog.ConfigurationId,
            FeatureFlagId = auditLog.FeatureFlagId,
            DeploymentId = auditLog.DeploymentId,
            Action = auditLog.Action,
            EntityType = auditLog.EntityType,
            OldValue = auditLog.OldValue,
            NewValue = auditLog.NewValue,
            UserId = auditLog.UserId,
            UserName = auditLog.UserName,
            Timestamp = auditLog.Timestamp,
            IpAddress = auditLog.IpAddress,
            UserAgent = auditLog.UserAgent,
            AdditionalData = auditLog.AdditionalData,
            TenantId = auditLog.TenantId
        };
    }
}
