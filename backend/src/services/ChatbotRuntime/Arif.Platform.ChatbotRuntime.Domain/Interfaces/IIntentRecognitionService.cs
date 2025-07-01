namespace Arif.Platform.ChatbotRuntime.Domain.Interfaces;

public interface IIntentRecognitionService
{
    Task<object> RecognizeIntentAsync(string message);
    Task<IEnumerable<object>> GetIntentsAsync();
    Task<object> CreateIntentAsync(object request);
    Task<object> UpdateIntentAsync(Guid intentId, object request);
    Task DeleteIntentAsync(Guid intentId);
}
