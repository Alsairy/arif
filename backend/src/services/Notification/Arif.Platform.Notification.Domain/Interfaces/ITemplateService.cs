namespace Arif.Platform.Notification.Domain.Interfaces;

public interface ITemplateService
{
    Task<IEnumerable<object>> GetTemplatesAsync(Guid tenantId);
    Task<object> GetTemplateAsync(Guid templateId, Guid tenantId);
    Task<object> CreateTemplateAsync(object request);
    Task<object> UpdateTemplateAsync(object request);
    Task<bool> DeleteTemplateAsync(Guid templateId, Guid tenantId);
}
