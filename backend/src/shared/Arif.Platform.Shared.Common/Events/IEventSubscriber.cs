namespace Arif.Platform.Shared.Common.Events;

public interface IEventSubscriber
{
    Task SubscribeAsync<T>(string eventType, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;
    Task SubscribeAsync(string eventType, Func<object, Task> handler, CancellationToken cancellationToken = default);
    Task UnsubscribeAsync(string eventType, CancellationToken cancellationToken = default);
}
