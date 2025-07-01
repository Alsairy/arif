namespace Arif.Platform.Notification.Domain.Interfaces;

public interface ISmsService
{
    Task<object> SendSmsAsync(object request);
    Task<IEnumerable<object>> GetSmsHistoryAsync();
    Task<object> GetSmsStatusAsync(Guid smsId);
    Task<object> CreateSmsTemplateAsync(object request);
    Task<IEnumerable<object>> GetSmsTemplatesAsync();
}
