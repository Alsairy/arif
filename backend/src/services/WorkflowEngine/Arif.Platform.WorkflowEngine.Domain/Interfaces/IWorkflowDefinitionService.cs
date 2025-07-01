namespace Arif.Platform.WorkflowEngine.Domain.Interfaces;

public interface IWorkflowDefinitionService
{
    Task<IEnumerable<object>> GetWorkflowDefinitionsAsync(object request);
    Task<object> GetWorkflowDefinitionAsync(Guid id, Guid tenantId);
    Task<object> CreateWorkflowDefinitionAsync(object request);
    Task<object> UpdateWorkflowDefinitionAsync(Guid id, object request);
    Task<bool> DeleteWorkflowDefinitionAsync(Guid id, Guid tenantId);
    Task<object> ValidateWorkflowAsync(object request, Guid? tenantId);
}
