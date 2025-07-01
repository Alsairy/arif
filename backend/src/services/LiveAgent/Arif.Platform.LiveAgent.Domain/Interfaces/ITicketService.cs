namespace Arif.Platform.LiveAgent.Domain.Interfaces;

public interface ITicketService
{
    Task<IEnumerable<object>> GetTicketsAsync(object request);
    Task<object> GetTicketAsync(Guid ticketId, Guid tenantId);
    Task<object> CreateTicketAsync(object request);
    Task<object> UpdateTicketAsync(Guid ticketId, object request);
    Task<object> AssignTicketAsync(Guid ticketId, object request);
    Task DeleteTicketAsync(Guid ticketId);
}
