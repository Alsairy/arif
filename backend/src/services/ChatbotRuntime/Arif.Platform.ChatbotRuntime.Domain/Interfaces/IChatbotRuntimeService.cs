namespace Arif.Platform.ChatbotRuntime.Domain.Interfaces;

public interface IChatbotRuntimeService
{
    Task<object> ProcessChatMessageAsync(object request);
    Task<object> ProcessMessageAsync(object request);
    Task<object> GetChatbotStatusAsync(Guid chatbotId);
    Task<object> StartChatbotAsync(Guid chatbotId);
    Task<object> StopChatbotAsync(Guid chatbotId);
}
