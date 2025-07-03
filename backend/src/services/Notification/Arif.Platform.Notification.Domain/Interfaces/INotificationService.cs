namespace Arif.Platform.Notification.Domain.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<object>> GetNotificationsAsync();
    Task<object> GetNotificationAsync(Guid notificationId, Guid tenantId);
    Task<object> CreateNotificationAsync(object request);
    Task<object> SendNotificationAsync(object request);
    Task<object> GetNotificationHistoryAsync(object request);
    Task<object> GetNotificationStatisticsAsync(object request);
    Task DeleteNotificationAsync(Guid notificationId);
}
