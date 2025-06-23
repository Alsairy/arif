using Arif.Platform.IntegrationGateway.Domain.Models;

namespace Arif.Platform.IntegrationGateway.Domain.Interfaces;

public interface IIntegrationService
{
    Task<bool> TestConnectionAsync(IntegrationConfiguration config);
    Task<string> SendMessageAsync(IntegrationConfiguration config, IntegrationMessage message);
    Task<bool> ProcessWebhookAsync(IntegrationConfiguration config, string payload);
}

public interface ITwilioService : IIntegrationService
{
    Task<string> SendSmsAsync(TwilioConfiguration config, string to, string message);
    Task<string> SendWhatsAppAsync(TwilioConfiguration config, string to, string message);
    Task<bool> ProcessTwilioWebhookAsync(string payload);
}

public interface IFacebookService : IIntegrationService
{
    Task<string> SendFacebookMessageAsync(FacebookConfiguration config, string recipientId, string message);
    Task<bool> ProcessFacebookWebhookAsync(string payload, string verifyToken);
    Task<bool> VerifyWebhookAsync(string mode, string token, string challenge, string verifyToken);
}

public interface ISlackService : IIntegrationService
{
    Task<string> SendSlackMessageAsync(SlackConfiguration config, string channel, string message);
    Task<bool> ProcessSlackWebhookAsync(string payload, string signature, string timestamp);
    Task<string> HandleSlackOAuthAsync(string code, SlackConfiguration config);
}

public interface ISalesforceService : IIntegrationService
{
    Task<string> CreateLeadAsync(SalesforceConfiguration config, Dictionary<string, object> leadData);
    Task<string> CreateContactAsync(SalesforceConfiguration config, Dictionary<string, object> contactData);
    Task<List<Dictionary<string, object>>> QueryRecordsAsync(SalesforceConfiguration config, string soqlQuery);
}

public interface IHubSpotService : IIntegrationService
{
    Task<string> CreateContactAsync(HubSpotConfiguration config, Dictionary<string, object> contactData);
    Task<string> CreateDealAsync(HubSpotConfiguration config, Dictionary<string, object> dealData);
    Task<List<Dictionary<string, object>>> GetContactsAsync(HubSpotConfiguration config, int limit = 100);
}
