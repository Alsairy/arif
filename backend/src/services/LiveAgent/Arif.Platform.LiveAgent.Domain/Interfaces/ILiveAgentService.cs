namespace Arif.Platform.LiveAgent.Domain.Interfaces;

public interface ILiveAgentService
{
    Task<object> GetAgentStatusAsync(Guid agentId);
    Task<object> UpdateAgentStatusAsync(Guid agentId, string status);
    Task<IEnumerable<object>> GetActiveChatsAsync(Guid agentId);
    Task<object> TransferChatAsync(Guid chatId, Guid targetAgentId);
    Task<object> GetAgentPerformanceAsync(Guid agentId, Guid tenantId);
}
