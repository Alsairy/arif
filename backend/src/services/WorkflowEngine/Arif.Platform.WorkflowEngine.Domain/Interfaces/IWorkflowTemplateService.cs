namespace Arif.Platform.WorkflowEngine.Domain.Interfaces;

public interface IWorkflowTemplateService
{
    Task<IEnumerable<object>> GetWorkflowTemplatesAsync();
    Task<object> GetWorkflowTemplateAsync(Guid id);
    Task<object> CreateWorkflowTemplateAsync(object request);
    Task<object> UpdateWorkflowTemplateAsync(Guid id, object request);
    Task DeleteWorkflowTemplateAsync(Guid id);
}
