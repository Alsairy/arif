namespace Arif.Platform.Subscription.Domain.Interfaces;

public interface ISubscriptionService
{
    Task<IEnumerable<object>> GetSubscriptionsAsync();
    Task<object> GetSubscriptionAsync(Guid id);
    Task<object> GetCurrentSubscriptionAsync(Guid tenantId);
    Task<object> CreateSubscriptionAsync(object request);
    Task<object> UpdateSubscriptionAsync(Guid id, object request);
    Task<object> UpgradeSubscriptionAsync(object request);
    Task<bool> CancelSubscriptionAsync(object request);
}
