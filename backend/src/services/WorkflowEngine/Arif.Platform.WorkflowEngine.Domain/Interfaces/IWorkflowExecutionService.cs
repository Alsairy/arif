namespace Arif.Platform.WorkflowEngine.Domain.Interfaces;

public interface IWorkflowExecutionService
{
    Task<object> ExecuteWorkflowAsync(Guid workflowId, object request);
    Task<IEnumerable<object>> GetWorkflowExecutionsAsync(object request);
    Task<object> GetWorkflowExecutionAsync(Guid executionId, Guid tenantId);
    Task<object> GetWorkflowExecutionStatusAsync(Guid executionId);
    Task<bool> CancelWorkflowExecutionAsync(Guid executionId, Guid? tenantId);
}
