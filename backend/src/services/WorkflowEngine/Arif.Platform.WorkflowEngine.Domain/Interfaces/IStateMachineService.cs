namespace Arif.Platform.WorkflowEngine.Domain.Interfaces;

public interface IStateMachineService
{
    Task<object> CreateStateMachineAsync(object request);
    Task<object> UpdateStateMachineAsync(Guid id, object request);
    Task<object> GetStateMachineAsync(Guid id);
    Task<IEnumerable<object>> GetStateMachinesAsync();
}
