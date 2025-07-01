namespace Arif.Platform.LiveAgent.Domain.Interfaces;

public interface IEscalationService
{
    Task<object> EscalateTicketAsync(Guid ticketId, object request);
    Task<IEnumerable<object>> GetEscalationRulesAsync();
    Task<object> CreateEscalationRuleAsync(object request);
    Task<object> UpdateEscalationRuleAsync(Guid ruleId, object request);
}
