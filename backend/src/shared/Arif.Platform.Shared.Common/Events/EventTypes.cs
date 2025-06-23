namespace Arif.Platform.Shared.Common.Events;

public static class EventTypes
{
    public const string UserRegistered = "user.registered";
    public const string UserLoggedIn = "user.logged_in";
    public const string UserLoggedOut = "user.logged_out";
    public const string UserUpdated = "user.updated";
    public const string UserDeleted = "user.deleted";

    public const string TenantCreated = "tenant.created";
    public const string TenantUpdated = "tenant.updated";
    public const string TenantDeleted = "tenant.deleted";
    public const string TenantSubscriptionChanged = "tenant.subscription_changed";

    public const string ChatbotCreated = "chatbot.created";
    public const string ChatbotUpdated = "chatbot.updated";
    public const string ChatbotDeleted = "chatbot.deleted";
    public const string ChatbotDeployed = "chatbot.deployed";

    public const string ChatSessionStarted = "chat.session_started";
    public const string ChatSessionEnded = "chat.session_ended";
    public const string ChatMessageReceived = "chat.message_received";
    public const string ChatMessageSent = "chat.message_sent";
    public const string ChatTransferredToAgent = "chat.transferred_to_agent";

    public const string IntegrationConnected = "integration.connected";
    public const string IntegrationDisconnected = "integration.disconnected";
    public const string IntegrationError = "integration.error";
    public const string WebhookReceived = "webhook.received";

    public const string AnalyticsDataGenerated = "analytics.data_generated";
    public const string ReportGenerated = "report.generated";

    public const string SystemHealthCheck = "system.health_check";
    public const string SystemError = "system.error";
    public const string SystemMaintenance = "system.maintenance";
}
