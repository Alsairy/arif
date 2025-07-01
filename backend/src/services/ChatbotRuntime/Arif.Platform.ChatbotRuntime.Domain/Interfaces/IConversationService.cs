namespace Arif.Platform.ChatbotRuntime.Domain.Interfaces;

public interface IConversationService
{
    Task<IEnumerable<object>> GetConversationsAsync(object request);
    Task<object> GetConversationAsync(Guid conversationId, Guid tenantId);
    Task<object> CreateConversationAsync(object request);
    Task<object> UpdateConversationAsync(Guid conversationId, object request);
    Task<IEnumerable<object>> GetConversationMessagesAsync(object request);
    Task DeleteConversationAsync(Guid conversationId);
}
