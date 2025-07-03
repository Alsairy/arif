namespace Arif.Platform.Subscription.Domain.Interfaces;

public interface IPlanManagementService
{
    Task<IEnumerable<object>> GetAvailablePlansAsync();
    Task<IEnumerable<object>> GetPlansAsync();
    Task<object> GetPlanAsync(Guid planId);
    Task<object> CreatePlanAsync(object request);
    Task<object> UpdatePlanAsync(Guid planId, object request);
    Task DeletePlanAsync(Guid planId);
}
