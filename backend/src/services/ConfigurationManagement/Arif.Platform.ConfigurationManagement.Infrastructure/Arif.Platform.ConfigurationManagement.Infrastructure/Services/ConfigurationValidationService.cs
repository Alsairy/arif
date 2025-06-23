using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Arif.Platform.ConfigurationManagement.Domain.Entities;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Domain.DTOs;

namespace Arif.Platform.ConfigurationManagement.Infrastructure.Services;

public class ConfigurationValidationService : IConfigurationValidationService
{
    private readonly ILogger<ConfigurationValidationService> _logger;

    public ConfigurationValidationService(ILogger<ConfigurationValidationService> logger)
    {
        _logger = logger;
    }

    public async Task<ConfigurationValidationResult> ValidateConfigurationAsync(Configuration configuration)
    {
        var result = new ConfigurationValidationResult
        {
            ConfigurationId = configuration.Id,
            ConfigurationKey = configuration.Key,
            IsValid = true,
            Errors = new List<string>(),
            Warnings = new List<string>()
        };

        if (string.IsNullOrWhiteSpace(configuration.Key))
        {
            result.Errors.Add("Configuration key is required");
            result.IsValid = false;
        }

        if (string.IsNullOrWhiteSpace(configuration.Value))
        {
            result.Errors.Add("Configuration value is required");
            result.IsValid = false;
        }

        if (string.IsNullOrWhiteSpace(configuration.Environment))
        {
            result.Errors.Add("Environment is required");
            result.IsValid = false;
        }

        if (string.IsNullOrWhiteSpace(configuration.Application))
        {
            result.Errors.Add("Application is required");
            result.IsValid = false;
        }

        if (configuration.ValidationRule != null)
        {
            var ruleValidation = await ValidateConfigurationValueAsync(configuration.Value, configuration.ValidationRule);
            if (!ruleValidation)
            {
                result.Errors.Add(configuration.ValidationRule.ErrorMessage);
                result.IsValid = false;
            }
        }

        if (configuration.Key.Length > 200)
        {
            result.Errors.Add("Configuration key cannot exceed 200 characters");
            result.IsValid = false;
        }

        if (configuration.Value.Length > 10000)
        {
            result.Warnings.Add("Configuration value is very long (>10000 characters)");
        }

        await Task.CompletedTask;
        return result;
    }

    public async Task<List<ConfigurationValidationResult>> ValidateConfigurationsAsync(List<Configuration> configurations)
    {
        var results = new List<ConfigurationValidationResult>();
        
        foreach (var configuration in configurations)
        {
            var result = await ValidateConfigurationAsync(configuration);
            results.Add(result);
        }

        var duplicateKeys = configurations
            .GroupBy(c => new { c.Key, c.Environment, c.Application, c.TenantId })
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicate in duplicateKeys)
        {
            var duplicateConfigs = configurations
                .Where(c => c.Key == duplicate.Key && 
                           c.Environment == duplicate.Environment && 
                           c.Application == duplicate.Application && 
                           c.TenantId == duplicate.TenantId);

            foreach (var config in duplicateConfigs)
            {
                var existingResult = results.FirstOrDefault(r => r.ConfigurationId == config.Id);
                if (existingResult != null)
                {
                    existingResult.Errors.Add($"Duplicate configuration key: {config.Key}");
                    existingResult.IsValid = false;
                }
            }
        }

        return results;
    }

    public async Task<bool> ValidateConfigurationValueAsync(string value, ConfigurationValidationRule rule)
    {
        if (rule.IsRequired && string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!rule.IsRequired && string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        switch (rule.RuleType.ToLower())
        {
            case "string":
                return ValidateStringRule(value, rule);
            case "number":
                return ValidateNumberRule(value, rule);
            case "boolean":
                return ValidateBooleanRule(value);
            case "email":
                return ValidateEmailRule(value);
            case "url":
                return ValidateUrlRule(value);
            case "json":
                return ValidateJsonRule(value);
            case "regex":
                return ValidateRegexRule(value, rule);
            case "allowed_values":
                return ValidateAllowedValuesRule(value, rule);
            default:
                _logger.LogWarning("Unknown validation rule type: {RuleType}", rule.RuleType);
                return true;
        }
    }

    public async Task<ConfigurationValidationRule> CreateValidationRuleAsync(CreateValidationRuleDto dto)
    {
        var rule = new ConfigurationValidationRule
        {
            Id = Guid.NewGuid(),
            RuleType = dto.RuleType,
            RuleExpression = dto.RuleExpression,
            ErrorMessage = dto.ErrorMessage,
            IsRequired = dto.IsRequired,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            AllowedValues = dto.AllowedValues,
            RegexPattern = dto.RegexPattern
        };

        await Task.CompletedTask;
        return rule;
    }

    public async Task<ConfigurationValidationRule> UpdateValidationRuleAsync(Guid id, UpdateValidationRuleDto dto)
    {
        var rule = new ConfigurationValidationRule
        {
            Id = id,
            RuleExpression = dto.RuleExpression ?? string.Empty,
            ErrorMessage = dto.ErrorMessage ?? string.Empty,
            IsRequired = dto.IsRequired ?? false,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            AllowedValues = dto.AllowedValues ?? new List<string>(),
            RegexPattern = dto.RegexPattern
        };

        await Task.CompletedTask;
        return rule;
    }

    public async Task<bool> DeleteValidationRuleAsync(Guid id)
    {
        await Task.CompletedTask;
        return true;
    }

    private static bool ValidateStringRule(string value, ConfigurationValidationRule rule)
    {
        if (!string.IsNullOrEmpty(rule.MinValue) && int.TryParse(rule.MinValue, out var minLength))
        {
            if (value.Length < minLength)
                return false;
        }

        if (!string.IsNullOrEmpty(rule.MaxValue) && int.TryParse(rule.MaxValue, out var maxLength))
        {
            if (value.Length > maxLength)
                return false;
        }

        return true;
    }

    private static bool ValidateNumberRule(string value, ConfigurationValidationRule rule)
    {
        if (!double.TryParse(value, out var numValue))
            return false;

        if (!string.IsNullOrEmpty(rule.MinValue) && double.TryParse(rule.MinValue, out var minValue))
        {
            if (numValue < minValue)
                return false;
        }

        if (!string.IsNullOrEmpty(rule.MaxValue) && double.TryParse(rule.MaxValue, out var maxValue))
        {
            if (numValue > maxValue)
                return false;
        }

        return true;
    }

    private static bool ValidateBooleanRule(string value)
    {
        return bool.TryParse(value, out _);
    }

    private static bool ValidateEmailRule(string value)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return addr.Address == value;
        }
        catch
        {
            return false;
        }
    }

    private static bool ValidateUrlRule(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }

    private static bool ValidateJsonRule(string value)
    {
        try
        {
            System.Text.Json.JsonDocument.Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool ValidateRegexRule(string value, ConfigurationValidationRule rule)
    {
        if (string.IsNullOrEmpty(rule.RegexPattern))
            return true;

        try
        {
            return Regex.IsMatch(value, rule.RegexPattern);
        }
        catch
        {
            return false;
        }
    }

    private static bool ValidateAllowedValuesRule(string value, ConfigurationValidationRule rule)
    {
        return rule.AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
    }
}
