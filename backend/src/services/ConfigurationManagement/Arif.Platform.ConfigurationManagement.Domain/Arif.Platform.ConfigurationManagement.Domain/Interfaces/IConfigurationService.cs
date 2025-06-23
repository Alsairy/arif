using Arif.Platform.ConfigurationManagement.Domain.Entities;
using Arif.Platform.ConfigurationManagement.Domain.DTOs;

namespace Arif.Platform.ConfigurationManagement.Domain.Interfaces;

public interface IConfigurationService
{
    Task<ConfigurationDto> GetConfigurationAsync(string key, string environment, string application, Guid? tenantId = null);
    Task<List<ConfigurationDto>> GetConfigurationsAsync(string environment, string application, Guid? tenantId = null);
    Task<ConfigurationDto> CreateConfigurationAsync(CreateConfigurationDto dto);
    Task<ConfigurationDto> UpdateConfigurationAsync(Guid id, UpdateConfigurationDto dto);
    Task<bool> DeleteConfigurationAsync(Guid id);
    Task<bool> ValidateConfigurationAsync(Guid id);
    Task<List<ConfigurationValidationResult>> ValidateConfigurationsAsync(string environment, string application);
    Task<DTOs.ConfigurationSnapshot> CreateSnapshotAsync(CreateSnapshotDto dto);
    Task<bool> RestoreFromSnapshotAsync(Guid snapshotId);
    Task<List<DTOs.ConfigurationSnapshot>> GetSnapshotsAsync(string environment, string application);
}

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string flagName, string environment, string application, Dictionary<string, object>? context = null, Guid? tenantId = null);
    Task<FeatureFlagDto> GetFeatureFlagAsync(string name, string environment, string application, Guid? tenantId = null);
    Task<List<FeatureFlagDto>> GetFeatureFlagsAsync(string environment, string application, Guid? tenantId = null);
    Task<FeatureFlagDto> CreateFeatureFlagAsync(CreateFeatureFlagDto dto);
    Task<FeatureFlagDto> UpdateFeatureFlagAsync(Guid id, UpdateFeatureFlagDto dto);
    Task<bool> DeleteFeatureFlagAsync(Guid id);
    Task<bool> ToggleFeatureFlagAsync(Guid id, bool enabled);
    Task<List<FeatureFlagEvaluationResult>> EvaluateRulesAsync(Guid flagId, Dictionary<string, object> context);
}

public interface IConfigurationDeploymentService
{
    Task<ConfigurationDeploymentDto> CreateDeploymentAsync(CreateDeploymentDto dto);
    Task<ConfigurationDeploymentDto> GetDeploymentAsync(Guid id);
    Task<List<ConfigurationDeploymentDto>> GetDeploymentsAsync(string environment, string application);
    Task<bool> ExecuteDeploymentAsync(Guid id);
    Task<bool> RollbackDeploymentAsync(Guid id, string reason);
    Task<bool> CancelDeploymentAsync(Guid id);
    Task<ConfigurationDeploymentStatus> GetDeploymentStatusAsync(Guid id);
    Task<List<ConfigurationDeploymentItem>> GetDeploymentItemsAsync(Guid deploymentId);
}

public interface IConfigurationAuditService
{
    Task LogConfigurationChangeAsync(Guid configurationId, string action, string? oldValue, string? newValue, string userId);
    Task LogFeatureFlagChangeAsync(Guid featureFlagId, string action, string? oldValue, string? newValue, string userId);
    Task LogDeploymentActionAsync(Guid deploymentId, string action, string userId);
    Task<List<ConfigurationAuditLogDto>> GetAuditLogsAsync(AuditLogFilter filter);
    Task<List<ConfigurationAuditLogDto>> GetConfigurationAuditLogsAsync(Guid configurationId);
    Task<List<ConfigurationAuditLogDto>> GetFeatureFlagAuditLogsAsync(Guid featureFlagId);
    Task<List<ConfigurationAuditLogDto>> GetDeploymentAuditLogsAsync(Guid deploymentId);
}

public interface IConfigurationValidationService
{
    Task<ConfigurationValidationResult> ValidateConfigurationAsync(Configuration configuration);
    Task<List<ConfigurationValidationResult>> ValidateConfigurationsAsync(List<Configuration> configurations);
    Task<bool> ValidateConfigurationValueAsync(string value, ConfigurationValidationRule rule);
    Task<ConfigurationValidationRule> CreateValidationRuleAsync(CreateValidationRuleDto dto);
    Task<ConfigurationValidationRule> UpdateValidationRuleAsync(Guid id, UpdateValidationRuleDto dto);
    Task<bool> DeleteValidationRuleAsync(Guid id);
}

public interface IConfigurationRepository
{
    Task<Configuration?> GetByKeyAsync(string key, string environment, string application, Guid? tenantId = null);
    Task<Configuration?> GetByIdAsync(Guid id);
    Task<List<Configuration>> GetAllAsync(string environment, string application, Guid? tenantId = null);
    Task<Configuration> CreateAsync(Configuration configuration);
    Task<Configuration> UpdateAsync(Configuration configuration);
    Task<bool> DeleteAsync(Guid id);
    Task<List<Configuration>> GetByTagsAsync(List<string> tags, string environment, string application);
}

public interface IFeatureFlagRepository
{
    Task<FeatureFlag?> GetByNameAsync(string name, string environment, string application, Guid? tenantId = null);
    Task<FeatureFlag?> GetByIdAsync(Guid id);
    Task<List<FeatureFlag>> GetAllAsync(string environment, string application, Guid? tenantId = null);
    Task<FeatureFlag> CreateAsync(FeatureFlag featureFlag);
    Task<FeatureFlag> UpdateAsync(FeatureFlag featureFlag);
    Task<bool> DeleteAsync(Guid id);
}

public interface IConfigurationDeploymentRepository
{
    Task<ConfigurationDeployment?> GetByIdAsync(Guid id);
    Task<List<ConfigurationDeployment>> GetAllAsync(string environment, string application);
    Task<ConfigurationDeployment> CreateAsync(ConfigurationDeployment deployment);
    Task<ConfigurationDeployment> UpdateAsync(ConfigurationDeployment deployment);
    Task<bool> DeleteAsync(Guid id);
}

public interface IConfigurationAuditRepository
{
    Task<ConfigurationAuditLog> CreateAsync(ConfigurationAuditLog auditLog);
    Task<List<ConfigurationAuditLog>> GetByFilterAsync(AuditLogFilter filter);
    Task<List<ConfigurationAuditLog>> GetByConfigurationIdAsync(Guid configurationId);
    Task<List<ConfigurationAuditLog>> GetByFeatureFlagIdAsync(Guid featureFlagId);
    Task<List<ConfigurationAuditLog>> GetByDeploymentIdAsync(Guid deploymentId);
}

public interface IConfigurationSnapshotRepository
{
    Task<Entities.ConfigurationSnapshot?> GetByIdAsync(Guid id);
    Task<List<Entities.ConfigurationSnapshot>> GetAllAsync(string environment, string application);
    Task<Entities.ConfigurationSnapshot> CreateAsync(Entities.ConfigurationSnapshot snapshot);
    Task<bool> DeleteAsync(Guid id);
}
