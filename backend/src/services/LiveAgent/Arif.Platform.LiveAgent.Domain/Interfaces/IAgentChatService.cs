namespace Arif.Platform.LiveAgent.Domain.Interfaces;

public interface IAgentChatService
{
    Task<IEnumerable<object>> GetChatHistoryAsync(Guid chatId);
    Task<object> SendMessageAsync(Guid chatId, object message);
    Task<object> JoinChatAsync(Guid chatId, Guid agentId);
    Task<object> JoinConversationAsync(Guid conversationId, Guid agentId);
    Task<IEnumerable<object>> GetAgentConversationsAsync(Guid agentId, Guid tenantId);
    Task LeaveChatAsync(Guid chatId, Guid agentId);
}
