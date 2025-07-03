namespace Arif.Platform.LiveAgent.Domain.Interfaces;

public interface IAgentManagementService
{
    Task<IEnumerable<object>> GetAgentsAsync(Guid tenantId);
    Task<object> GetAgentAsync(Guid agentId, Guid tenantId);
    Task<object> CreateAgentAsync(object request);
    Task<object> UpdateAgentAsync(Guid agentId, object request);
    Task<object> UpdateAgentStatusAsync(Guid agentId, object request);
    Task DeleteAgentAsync(Guid agentId);
}
