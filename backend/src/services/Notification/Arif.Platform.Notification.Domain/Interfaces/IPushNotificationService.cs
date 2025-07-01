namespace Arif.Platform.Notification.Domain.Interfaces;

public interface IPushNotificationService
{
    Task<object> SendPushNotificationAsync(object request);
    Task<IEnumerable<object>> GetPushNotificationHistoryAsync();
    Task<object> GetPushNotificationStatusAsync(Guid notificationId);
    Task<object> RegisterDeviceAsync(object request);
    Task UnregisterDeviceAsync(Guid deviceId);
}
