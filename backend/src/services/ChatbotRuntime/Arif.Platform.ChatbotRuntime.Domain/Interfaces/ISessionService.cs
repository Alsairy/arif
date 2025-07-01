namespace Arif.Platform.ChatbotRuntime.Domain.Interfaces;

public interface ISessionService
{
    Task<object> CreateSessionAsync(object request);
    Task<object> GetSessionAsync(Guid sessionId, Guid tenantId);
    Task<IEnumerable<object>> GetSessionsAsync();
    Task<object> UpdateSessionAsync(Guid sessionId, object request);
    Task<bool> EndSessionAsync(Guid sessionId, Guid tenantId);
    Task DeleteSessionAsync(Guid sessionId);
}
