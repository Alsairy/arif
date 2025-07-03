namespace Arif.Platform.Notification.Domain.Interfaces;

public interface IEmailService
{
    Task<object> SendEmailAsync(object request);
    Task<IEnumerable<object>> GetEmailTemplatesAsync();
    Task<object> CreateEmailTemplateAsync(object request);
    Task<object> UpdateEmailTemplateAsync(Guid templateId, object request);
    Task DeleteEmailTemplateAsync(Guid templateId);
}
