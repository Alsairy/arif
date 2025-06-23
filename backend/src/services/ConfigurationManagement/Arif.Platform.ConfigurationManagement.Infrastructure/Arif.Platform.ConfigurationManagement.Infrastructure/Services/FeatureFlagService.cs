using Microsoft.Extensions.Logging;
using Arif.Platform.ConfigurationManagement.Domain.Entities;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Domain.DTOs;

namespace Arif.Platform.ConfigurationManagement.Infrastructure.Services;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureFlagRepository _featureFlagRepository;
    private readonly IConfigurationAuditService _auditService;
    private readonly ILogger<FeatureFlagService> _logger;

    public FeatureFlagService(
        IFeatureFlagRepository featureFlagRepository,
        IConfigurationAuditService auditService,
        ILogger<FeatureFlagService> logger)
    {
        _featureFlagRepository = featureFlagRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string flagName, string environment, string application, Dictionary<string, object>? context = null, Guid? tenantId = null)
    {
        var featureFlag = await _featureFlagRepository.GetByNameAsync(flagName, environment, application, tenantId);
        if (featureFlag == null)
        {
            _logger.LogWarning("Feature flag not found: {FlagName} in {Environment}/{Application}", flagName, environment, application);
            return false;
        }

        if (!featureFlag.IsEnabled)
            return false;

        if (featureFlag.Schedule != null && !IsScheduleActive(featureFlag.Schedule))
            return false;

        if (featureFlag.Rules.Any() && context != null)
        {
            var evaluationResults = await EvaluateRulesAsync(featureFlag.Id, context);
            return evaluationResults.Any(r => r.IsEnabled);
        }

        return featureFlag.IsEnabled;
    }

    public async Task<FeatureFlagDto> GetFeatureFlagAsync(string name, string environment, string application, Guid? tenantId = null)
    {
        var featureFlag = await _featureFlagRepository.GetByNameAsync(name, environment, application, tenantId);
        if (featureFlag == null)
            throw new KeyNotFoundException($"Feature flag '{name}' not found");

        return MapToDto(featureFlag);
    }

    public async Task<List<FeatureFlagDto>> GetFeatureFlagsAsync(string environment, string application, Guid? tenantId = null)
    {
        var featureFlags = await _featureFlagRepository.GetAllAsync(environment, application, tenantId);
        return featureFlags.Select(MapToDto).ToList();
    }

    public async Task<FeatureFlagDto> CreateFeatureFlagAsync(CreateFeatureFlagDto dto)
    {
        var featureFlag = new FeatureFlag
        {
            Name = dto.Name,
            Description = dto.Description,
            IsEnabled = dto.IsEnabled,
            Environment = dto.Environment,
            Application = dto.Application,
            TenantId = dto.TenantId,
            CreatedBy = "system",
            UpdatedBy = "system",
            Metadata = dto.Metadata,
            Rules = dto.Rules.Select(r => new FeatureFlagRule
            {
                RuleType = r.RuleType,
                Attribute = r.Attribute,
                Operator = r.Operator,
                Value = r.Value,
                Priority = r.Priority,
                IsActive = true
            }).ToList()
        };

        if (dto.Schedule != null)
        {
            featureFlag.Schedule = new FeatureFlagSchedule
            {
                StartDate = dto.Schedule.StartDate,
                EndDate = dto.Schedule.EndDate,
                CronExpression = dto.Schedule.CronExpression,
                TimeZone = dto.Schedule.TimeZone,
                IsActive = true
            };
        }

        var createdFeatureFlag = await _featureFlagRepository.CreateAsync(featureFlag);
        
        await _auditService.LogFeatureFlagChangeAsync(
            createdFeatureFlag.Id, 
            "CREATE", 
            null, 
            createdFeatureFlag.IsEnabled.ToString(), 
            createdFeatureFlag.CreatedBy);

        _logger.LogInformation("Feature flag created: {Name} in {Environment}/{Application}", 
            createdFeatureFlag.Name, createdFeatureFlag.Environment, createdFeatureFlag.Application);

        return MapToDto(createdFeatureFlag);
    }

    public async Task<FeatureFlagDto> UpdateFeatureFlagAsync(Guid id, UpdateFeatureFlagDto dto)
    {
        var featureFlag = await _featureFlagRepository.GetByIdAsync(id);
        if (featureFlag == null)
            throw new KeyNotFoundException($"Feature flag with ID '{id}' not found");

        var oldValue = featureFlag.IsEnabled.ToString();

        if (dto.Description != null)
            featureFlag.Description = dto.Description;
        if (dto.IsEnabled.HasValue)
            featureFlag.IsEnabled = dto.IsEnabled.Value;
        if (dto.Metadata != null)
            featureFlag.Metadata = dto.Metadata;

        featureFlag.UpdatedBy = "system";

        var updatedFeatureFlag = await _featureFlagRepository.UpdateAsync(featureFlag);
        
        await _auditService.LogFeatureFlagChangeAsync(
            updatedFeatureFlag.Id, 
            "UPDATE", 
            oldValue, 
            updatedFeatureFlag.IsEnabled.ToString(), 
            updatedFeatureFlag.UpdatedBy);

        _logger.LogInformation("Feature flag updated: {Name} in {Environment}/{Application}", 
            updatedFeatureFlag.Name, updatedFeatureFlag.Environment, updatedFeatureFlag.Application);

        return MapToDto(updatedFeatureFlag);
    }

    public async Task<bool> DeleteFeatureFlagAsync(Guid id)
    {
        var featureFlag = await _featureFlagRepository.GetByIdAsync(id);
        if (featureFlag == null)
            return false;

        var result = await _featureFlagRepository.DeleteAsync(id);
        
        if (result)
        {
            await _auditService.LogFeatureFlagChangeAsync(
                id, 
                "DELETE", 
                featureFlag.IsEnabled.ToString(), 
                null, 
                "system");

            _logger.LogInformation("Feature flag deleted: {Name} in {Environment}/{Application}", 
                featureFlag.Name, featureFlag.Environment, featureFlag.Application);
        }

        return result;
    }

    public async Task<bool> ToggleFeatureFlagAsync(Guid id, bool enabled)
    {
        var featureFlag = await _featureFlagRepository.GetByIdAsync(id);
        if (featureFlag == null)
            return false;

        var oldValue = featureFlag.IsEnabled.ToString();
        featureFlag.IsEnabled = enabled;
        featureFlag.UpdatedBy = "system";

        await _featureFlagRepository.UpdateAsync(featureFlag);
        
        await _auditService.LogFeatureFlagChangeAsync(
            id, 
            "TOGGLE", 
            oldValue, 
            enabled.ToString(), 
            "system");

        _logger.LogInformation("Feature flag toggled: {Name} to {Enabled} in {Environment}/{Application}", 
            featureFlag.Name, enabled, featureFlag.Environment, featureFlag.Application);

        return true;
    }

    public async Task<List<FeatureFlagEvaluationResult>> EvaluateRulesAsync(Guid flagId, Dictionary<string, object> context)
    {
        var featureFlag = await _featureFlagRepository.GetByIdAsync(flagId);
        if (featureFlag == null)
            return new List<FeatureFlagEvaluationResult>();

        var results = new List<FeatureFlagEvaluationResult>();

        foreach (var rule in featureFlag.Rules.Where(r => r.IsActive).OrderBy(r => r.Priority))
        {
            var result = EvaluateRule(rule, context);
            results.Add(result);
        }

        return results;
    }

    private static FeatureFlagEvaluationResult EvaluateRule(FeatureFlagRule rule, Dictionary<string, object> context)
    {
        var result = new FeatureFlagEvaluationResult
        {
            Context = context,
            MatchedRules = new List<string> { rule.RuleType }
        };

        if (!context.ContainsKey(rule.Attribute))
        {
            result.IsEnabled = false;
            result.Reason = $"Attribute '{rule.Attribute}' not found in context";
            return result;
        }

        var attributeValue = context[rule.Attribute];
        var ruleValue = rule.Value;

        try
        {
            result.IsEnabled = rule.Operator switch
            {
                "equals" => attributeValue.ToString() == ruleValue,
                "not_equals" => attributeValue.ToString() != ruleValue,
                "contains" => attributeValue.ToString()?.Contains(ruleValue) == true,
                "starts_with" => attributeValue.ToString()?.StartsWith(ruleValue) == true,
                "ends_with" => attributeValue.ToString()?.EndsWith(ruleValue) == true,
                "greater_than" => double.TryParse(attributeValue.ToString(), out var attrNum) && 
                                 double.TryParse(ruleValue, out var ruleNum) && attrNum > ruleNum,
                "less_than" => double.TryParse(attributeValue.ToString(), out var attrNum2) && 
                              double.TryParse(ruleValue, out var ruleNum2) && attrNum2 < ruleNum2,
                "percentage" => EvaluatePercentageRule(attributeValue.ToString(), ruleValue),
                _ => false
            };

            result.Reason = result.IsEnabled ? "Rule matched" : "Rule did not match";
        }
        catch (Exception ex)
        {
            result.IsEnabled = false;
            result.Reason = $"Rule evaluation error: {ex.Message}";
        }

        return result;
    }

    private static bool EvaluatePercentageRule(string? attributeValue, string ruleValue)
    {
        if (string.IsNullOrEmpty(attributeValue) || !int.TryParse(ruleValue, out var percentage))
            return false;

        var hash = attributeValue.GetHashCode();
        var bucket = Math.Abs(hash) % 100;
        return bucket < percentage;
    }

    private static bool IsScheduleActive(FeatureFlagSchedule schedule)
    {
        if (!schedule.IsActive)
            return false;

        var now = DateTime.UtcNow;
        
        if (schedule.StartDate.HasValue && now < schedule.StartDate.Value)
            return false;
            
        if (schedule.EndDate.HasValue && now > schedule.EndDate.Value)
            return false;

        return true;
    }

    private static FeatureFlagDto MapToDto(FeatureFlag featureFlag)
    {
        return new FeatureFlagDto
        {
            Id = featureFlag.Id,
            Name = featureFlag.Name,
            Description = featureFlag.Description,
            IsEnabled = featureFlag.IsEnabled,
            Environment = featureFlag.Environment,
            Application = featureFlag.Application,
            CreatedAt = featureFlag.CreatedAt,
            UpdatedAt = featureFlag.UpdatedAt,
            CreatedBy = featureFlag.CreatedBy,
            UpdatedBy = featureFlag.UpdatedBy,
            TenantId = featureFlag.TenantId,
            Metadata = featureFlag.Metadata,
            Rules = featureFlag.Rules.Select(r => new FeatureFlagRuleDto
            {
                Id = r.Id,
                RuleType = r.RuleType,
                Attribute = r.Attribute,
                Operator = r.Operator,
                Value = r.Value,
                Priority = r.Priority,
                IsActive = r.IsActive
            }).ToList(),
            Schedule = featureFlag.Schedule != null ? new FeatureFlagScheduleDto
            {
                Id = featureFlag.Schedule.Id,
                StartDate = featureFlag.Schedule.StartDate,
                EndDate = featureFlag.Schedule.EndDate,
                CronExpression = featureFlag.Schedule.CronExpression,
                TimeZone = featureFlag.Schedule.TimeZone,
                IsActive = featureFlag.Schedule.IsActive
            } : null
        };
    }
}
