using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.ConfigurationManagement.Domain.Entities;

public class Configuration
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public Guid? TenantId { get; set; }
    public List<string> Tags { get; set; } = new();
    public ConfigurationValidationRule? ValidationRule { get; set; }
}

public class ConfigurationValidationRule
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string RuleExpression { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? MinValue { get; set; }
    public string? MaxValue { get; set; }
    public List<string> AllowedValues { get; set; } = new();
    public string? RegexPattern { get; set; }
}

public class FeatureFlag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Environment { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public List<FeatureFlagRule> Rules { get; set; } = new();
    public FeatureFlagSchedule? Schedule { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class FeatureFlagRule
{
    public Guid Id { get; set; }
    public Guid FeatureFlagId { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string Attribute { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
}

public class FeatureFlagSchedule
{
    public Guid Id { get; set; }
    public Guid FeatureFlagId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CronExpression { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public bool IsActive { get; set; } = true;
}

public class ConfigurationDeployment
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public ConfigurationDeploymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeployedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? DeployedBy { get; set; }
    public List<ConfigurationDeploymentItem> Items { get; set; } = new();
    public string? RollbackReason { get; set; }
    public Guid? RollbackToDeploymentId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ConfigurationDeploymentItem
{
    public Guid Id { get; set; }
    public Guid DeploymentId { get; set; }
    public Guid ConfigurationId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public ConfigurationDeploymentItemStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class ConfigurationAuditLog
{
    public Guid Id { get; set; }
    public Guid? ConfigurationId { get; set; }
    public Guid? FeatureFlagId { get; set; }
    public Guid? DeploymentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    public Guid? TenantId { get; set; }
}

public class ConfigurationSnapshot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public Dictionary<string, string> ConfigurationData { get; set; } = new();
    public Dictionary<string, bool> FeatureFlagData { get; set; } = new();
    public string? Tags { get; set; }
    public bool IsAutomatic { get; set; }
}

public enum ConfigurationDeploymentStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    RolledBack,
    Cancelled
}

public enum ConfigurationDeploymentItemStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Skipped
}

public enum FeatureFlagRuleType
{
    UserAttribute,
    Percentage,
    DateTime,
    Custom
}

public enum ConfigurationValidationType
{
    Required,
    DataType,
    Range,
    Regex,
    AllowedValues,
    Custom
}
