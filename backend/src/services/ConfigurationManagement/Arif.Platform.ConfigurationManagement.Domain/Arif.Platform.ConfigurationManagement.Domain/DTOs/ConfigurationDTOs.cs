namespace Arif.Platform.ConfigurationManagement.Domain.DTOs;

public class ConfigurationDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public int Version { get; set; }
    public Guid? TenantId { get; set; }
    public List<string> Tags { get; set; } = new();
    public ConfigurationValidationRuleDto? ValidationRule { get; set; }
}

public class CreateConfigurationDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public List<string> Tags { get; set; } = new();
    public Guid? TenantId { get; set; }
    public CreateValidationRuleDto? ValidationRule { get; set; }
}

public class UpdateConfigurationDto
{
    public string? Value { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? Tags { get; set; }
    public UpdateValidationRuleDto? ValidationRule { get; set; }
}

public class ConfigurationValidationRuleDto
{
    public Guid Id { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string RuleExpression { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? MinValue { get; set; }
    public string? MaxValue { get; set; }
    public List<string> AllowedValues { get; set; } = new();
    public string? RegexPattern { get; set; }
}

public class CreateValidationRuleDto
{
    public string RuleType { get; set; } = string.Empty;
    public string RuleExpression { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? MinValue { get; set; }
    public string? MaxValue { get; set; }
    public List<string> AllowedValues { get; set; } = new();
    public string? RegexPattern { get; set; }
}

public class UpdateValidationRuleDto
{
    public string? RuleExpression { get; set; }
    public string? ErrorMessage { get; set; }
    public bool? IsRequired { get; set; }
    public string? MinValue { get; set; }
    public string? MaxValue { get; set; }
    public List<string>? AllowedValues { get; set; }
    public string? RegexPattern { get; set; }
}

public class FeatureFlagDto
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
    public List<FeatureFlagRuleDto> Rules { get; set; } = new();
    public FeatureFlagScheduleDto? Schedule { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class CreateFeatureFlagDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Environment { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public List<CreateFeatureFlagRuleDto> Rules { get; set; } = new();
    public CreateFeatureFlagScheduleDto? Schedule { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class UpdateFeatureFlagDto
{
    public string? Description { get; set; }
    public bool? IsEnabled { get; set; }
    public List<UpdateFeatureFlagRuleDto>? Rules { get; set; }
    public UpdateFeatureFlagScheduleDto? Schedule { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class FeatureFlagRuleDto
{
    public Guid Id { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string Attribute { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsActive { get; set; }
}

public class CreateFeatureFlagRuleDto
{
    public string RuleType { get; set; } = string.Empty;
    public string Attribute { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public class UpdateFeatureFlagRuleDto
{
    public Guid Id { get; set; }
    public string? RuleType { get; set; }
    public string? Attribute { get; set; }
    public string? Operator { get; set; }
    public string? Value { get; set; }
    public int? Priority { get; set; }
    public bool? IsActive { get; set; }
}

public class FeatureFlagScheduleDto
{
    public Guid Id { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CronExpression { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public bool IsActive { get; set; }
}

public class CreateFeatureFlagScheduleDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CronExpression { get; set; }
    public string TimeZone { get; set; } = "UTC";
}

public class UpdateFeatureFlagScheduleDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CronExpression { get; set; }
    public string? TimeZone { get; set; }
    public bool? IsActive { get; set; }
}

public class ConfigurationDeploymentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DeployedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? DeployedBy { get; set; }
    public List<ConfigurationDeploymentItemDto> Items { get; set; } = new();
    public string? RollbackReason { get; set; }
    public Guid? RollbackToDeploymentId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class CreateDeploymentDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public List<CreateDeploymentItemDto> Items { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ConfigurationDeploymentItemDto
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class CreateDeploymentItemDto
{
    public Guid ConfigurationId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? NewValue { get; set; }
}

public class ConfigurationAuditLogDto
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

public class CreateSnapshotDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public string? Tags { get; set; }
}

public class ConfigurationValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Guid ConfigurationId { get; set; }
    public string ConfigurationKey { get; set; } = string.Empty;
}

public class FeatureFlagEvaluationResult
{
    public bool IsEnabled { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<string> MatchedRules { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
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
    public string? Tags { get; set; }
    public List<ConfigurationDto> Configurations { get; set; } = new();
    public List<FeatureFlagDto> FeatureFlags { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class AuditLogFilter
{
    public Guid? ConfigurationId { get; set; }
    public Guid? FeatureFlagId { get; set; }
    public Guid? DeploymentId { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public string? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? TenantId { get; set; }
    public int PageSize { get; set; } = 50;
    public int PageNumber { get; set; } = 1;
}
