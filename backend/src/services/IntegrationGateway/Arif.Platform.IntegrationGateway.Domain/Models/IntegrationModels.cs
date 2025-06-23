using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.IntegrationGateway.Domain.Models;

public class IntegrationConfiguration
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string IntegrationType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public Dictionary<string, string> Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TwilioConfiguration
{
    [Required]
    public string AccountSid { get; set; } = string.Empty;
    
    [Required]
    public string AuthToken { get; set; } = string.Empty;
    
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string? WhatsAppNumber { get; set; }
    
    public string WebhookUrl { get; set; } = string.Empty;
}

public class FacebookConfiguration
{
    [Required]
    public string AppId { get; set; } = string.Empty;
    
    [Required]
    public string AppSecret { get; set; } = string.Empty;
    
    [Required]
    public string PageAccessToken { get; set; } = string.Empty;
    
    [Required]
    public string PageId { get; set; } = string.Empty;
    
    public string WebhookVerifyToken { get; set; } = string.Empty;
}

public class SlackConfiguration
{
    [Required]
    public string ClientId { get; set; } = string.Empty;
    
    [Required]
    public string ClientSecret { get; set; } = string.Empty;
    
    [Required]
    public string BotToken { get; set; } = string.Empty;
    
    [Required]
    public string SigningSecret { get; set; } = string.Empty;
    
    public string WebhookUrl { get; set; } = string.Empty;
}

public class SalesforceConfiguration
{
    [Required]
    public string ClientId { get; set; } = string.Empty;
    
    [Required]
    public string ClientSecret { get; set; } = string.Empty;
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public string SecurityToken { get; set; } = string.Empty;
    
    public string InstanceUrl { get; set; } = "https://login.salesforce.com";
}

public class HubSpotConfiguration
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;
    
    public string? AccessToken { get; set; }
    
    public string? RefreshToken { get; set; }
    
    public string PortalId { get; set; } = string.Empty;
}

public class WebhookEvent
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string IntegrationType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class IntegrationMessage
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "text";
    public Dictionary<string, object> Metadata { get; set; } = new();
}
