using Microsoft.Extensions.Logging;
using Arif.Platform.ConfigurationManagement.Domain.Entities;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Domain.DTOs;

namespace Arif.Platform.ConfigurationManagement.Infrastructure.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IConfigurationValidationService _validationService;
    private readonly IConfigurationAuditService _auditService;
    private readonly IConfigurationSnapshotRepository _snapshotRepository;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(
        IConfigurationRepository configurationRepository,
        IConfigurationValidationService validationService,
        IConfigurationAuditService auditService,
        IConfigurationSnapshotRepository snapshotRepository,
        ILogger<ConfigurationService> logger)
    {
        _configurationRepository = configurationRepository;
        _validationService = validationService;
        _auditService = auditService;
        _snapshotRepository = snapshotRepository;
        _logger = logger;
    }

    public async Task<ConfigurationDto> GetConfigurationAsync(string key, string environment, string application, Guid? tenantId = null)
    {
        var configuration = await _configurationRepository.GetByKeyAsync(key, environment, application, tenantId);
        if (configuration == null)
            throw new KeyNotFoundException($"Configuration with key '{key}' not found");

        return MapToDto(configuration);
    }

    public async Task<List<ConfigurationDto>> GetConfigurationsAsync(string environment, string application, Guid? tenantId = null)
    {
        var configurations = await _configurationRepository.GetAllAsync(environment, application, tenantId);
        return configurations.Select(MapToDto).ToList();
    }

    public async Task<ConfigurationDto> CreateConfigurationAsync(CreateConfigurationDto dto)
    {
        var configuration = new Configuration
        {
            Key = dto.Key,
            Value = dto.Value,
            Environment = dto.Environment,
            Application = dto.Application,
            Description = dto.Description,
            IsEncrypted = dto.IsEncrypted,
            Tags = dto.Tags,
            TenantId = dto.TenantId,
            CreatedBy = "system",
            UpdatedBy = "system"
        };

        if (dto.ValidationRule != null)
        {
            configuration.ValidationRule = new ConfigurationValidationRule
            {
                RuleType = dto.ValidationRule.RuleType,
                RuleExpression = dto.ValidationRule.RuleExpression,
                ErrorMessage = dto.ValidationRule.ErrorMessage,
                IsRequired = dto.ValidationRule.IsRequired,
                MinValue = dto.ValidationRule.MinValue,
                MaxValue = dto.ValidationRule.MaxValue,
                AllowedValues = dto.ValidationRule.AllowedValues,
                RegexPattern = dto.ValidationRule.RegexPattern
            };
        }

        var validationResult = await _validationService.ValidateConfigurationAsync(configuration);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Configuration validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        var createdConfiguration = await _configurationRepository.CreateAsync(configuration);
        
        await _auditService.LogConfigurationChangeAsync(
            createdConfiguration.Id, 
            "CREATE", 
            null, 
            createdConfiguration.Value, 
            createdConfiguration.CreatedBy);

        _logger.LogInformation("Configuration created: {Key} in {Environment}/{Application}", 
            createdConfiguration.Key, createdConfiguration.Environment, createdConfiguration.Application);

        return MapToDto(createdConfiguration);
    }

    public async Task<ConfigurationDto> UpdateConfigurationAsync(Guid id, UpdateConfigurationDto dto)
    {
        var configuration = await _configurationRepository.GetByIdAsync(id);
        if (configuration == null)
            throw new KeyNotFoundException($"Configuration with ID '{id}' not found");

        var oldValue = configuration.Value;

        if (dto.Value != null)
            configuration.Value = dto.Value;
        if (dto.Description != null)
            configuration.Description = dto.Description;
        if (dto.IsActive.HasValue)
            configuration.IsActive = dto.IsActive.Value;
        if (dto.Tags != null)
            configuration.Tags = dto.Tags;

        configuration.UpdatedBy = "system";

        var validationResult = await _validationService.ValidateConfigurationAsync(configuration);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Configuration validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        var updatedConfiguration = await _configurationRepository.UpdateAsync(configuration);
        
        await _auditService.LogConfigurationChangeAsync(
            updatedConfiguration.Id, 
            "UPDATE", 
            oldValue, 
            updatedConfiguration.Value, 
            updatedConfiguration.UpdatedBy);

        _logger.LogInformation("Configuration updated: {Key} in {Environment}/{Application}", 
            updatedConfiguration.Key, updatedConfiguration.Environment, updatedConfiguration.Application);

        return MapToDto(updatedConfiguration);
    }

    public async Task<bool> DeleteConfigurationAsync(Guid id)
    {
        var configuration = await _configurationRepository.GetByIdAsync(id);
        if (configuration == null)
            return false;

        var result = await _configurationRepository.DeleteAsync(id);
        
        if (result)
        {
            await _auditService.LogConfigurationChangeAsync(
                id, 
                "DELETE", 
                configuration.Value, 
                null, 
                "system");

            _logger.LogInformation("Configuration deleted: {Key} in {Environment}/{Application}", 
                configuration.Key, configuration.Environment, configuration.Application);
        }

        return result;
    }

    public async Task<bool> ValidateConfigurationAsync(Guid id)
    {
        var configuration = await _configurationRepository.GetByIdAsync(id);
        if (configuration == null)
            return false;

        var validationResult = await _validationService.ValidateConfigurationAsync(configuration);
        return validationResult.IsValid;
    }

    public async Task<List<ConfigurationValidationResult>> ValidateConfigurationsAsync(string environment, string application)
    {
        var configurations = await _configurationRepository.GetAllAsync(environment, application);
        return await _validationService.ValidateConfigurationsAsync(configurations);
    }

    public async Task<Arif.Platform.ConfigurationManagement.Domain.DTOs.ConfigurationSnapshot> CreateSnapshotAsync(CreateSnapshotDto dto)
    {
        var configurations = await _configurationRepository.GetAllAsync(dto.Environment, dto.Application);
        
        var entitySnapshot = new Arif.Platform.ConfigurationManagement.Domain.Entities.ConfigurationSnapshot
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Environment = dto.Environment,
            Application = dto.Application,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            Tags = dto.Tags,
            IsAutomatic = false,
            ConfigurationData = configurations.ToDictionary(c => c.Key, c => c.Value),
            FeatureFlagData = new Dictionary<string, bool>()
        };

        var createdSnapshot = await _snapshotRepository.CreateAsync(entitySnapshot);
        
        return new Arif.Platform.ConfigurationManagement.Domain.DTOs.ConfigurationSnapshot
        {
            Id = createdSnapshot.Id,
            Name = createdSnapshot.Name,
            Description = createdSnapshot.Description,
            Environment = createdSnapshot.Environment,
            Application = createdSnapshot.Application,
            CreatedAt = createdSnapshot.CreatedAt,
            CreatedBy = createdSnapshot.CreatedBy,
            Tags = createdSnapshot.Tags,
            Configurations = configurations.Select(MapToDto).ToList(),
            FeatureFlags = new List<FeatureFlagDto>(),
            Metadata = new Dictionary<string, object>()
        };
    }

    public async Task<bool> RestoreFromSnapshotAsync(Guid snapshotId)
    {
        var snapshot = await _snapshotRepository.GetByIdAsync(snapshotId);
        if (snapshot == null)
            return false;

        var currentConfigurations = await _configurationRepository.GetAllAsync(snapshot.Environment, snapshot.Application);
        
        foreach (var kvp in snapshot.ConfigurationData)
        {
            var existing = currentConfigurations.FirstOrDefault(c => c.Key == kvp.Key);
            if (existing != null)
            {
                existing.Value = kvp.Value;
                existing.UpdatedBy = "system";
                await _configurationRepository.UpdateAsync(existing);
                
                await _auditService.LogConfigurationChangeAsync(
                    existing.Id, 
                    "RESTORE", 
                    existing.Value, 
                    kvp.Value, 
                    "system");
            }
        }

        _logger.LogInformation("Configuration restored from snapshot: {SnapshotName} for {Environment}/{Application}", 
            snapshot.Name, snapshot.Environment, snapshot.Application);

        return true;
    }

    public async Task<List<Arif.Platform.ConfigurationManagement.Domain.DTOs.ConfigurationSnapshot>> GetSnapshotsAsync(string environment, string application)
    {
        var entitySnapshots = await _snapshotRepository.GetAllAsync(environment, application);
        var result = new List<Arif.Platform.ConfigurationManagement.Domain.DTOs.ConfigurationSnapshot>();
        
        foreach (var entitySnapshot in entitySnapshots)
        {
            var configurations = await _configurationRepository.GetAllAsync(entitySnapshot.Environment, entitySnapshot.Application);
            
            result.Add(new Arif.Platform.ConfigurationManagement.Domain.DTOs.ConfigurationSnapshot
            {
                Id = entitySnapshot.Id,
                Name = entitySnapshot.Name,
                Description = entitySnapshot.Description,
                Environment = entitySnapshot.Environment,
                Application = entitySnapshot.Application,
                CreatedAt = entitySnapshot.CreatedAt,
                CreatedBy = entitySnapshot.CreatedBy,
                Tags = entitySnapshot.Tags,
                Configurations = configurations.Select(MapToDto).ToList(),
                FeatureFlags = new List<FeatureFlagDto>(),
                Metadata = new Dictionary<string, object>()
            });
        }
        
        return result;
    }

    private static ConfigurationDto MapToDto(Configuration configuration)
    {
        return new ConfigurationDto
        {
            Id = configuration.Id,
            Key = configuration.Key,
            Value = configuration.Value,
            Environment = configuration.Environment,
            Application = configuration.Application,
            Description = configuration.Description,
            IsEncrypted = configuration.IsEncrypted,
            IsActive = configuration.IsActive,
            CreatedAt = configuration.CreatedAt,
            UpdatedAt = configuration.UpdatedAt,
            CreatedBy = configuration.CreatedBy,
            UpdatedBy = configuration.UpdatedBy,
            Version = configuration.Version,
            TenantId = configuration.TenantId,
            Tags = configuration.Tags,
            ValidationRule = configuration.ValidationRule != null ? new ConfigurationValidationRuleDto
            {
                Id = configuration.ValidationRule.Id,
                RuleType = configuration.ValidationRule.RuleType,
                RuleExpression = configuration.ValidationRule.RuleExpression,
                ErrorMessage = configuration.ValidationRule.ErrorMessage,
                IsRequired = configuration.ValidationRule.IsRequired,
                MinValue = configuration.ValidationRule.MinValue,
                MaxValue = configuration.ValidationRule.MaxValue,
                AllowedValues = configuration.ValidationRule.AllowedValues,
                RegexPattern = configuration.ValidationRule.RegexPattern
            } : null
        };
    }
}
