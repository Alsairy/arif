namespace Arif.Platform.Shared.Common.Events;

public interface IEventPublisher
{
    Task PublishAsync<T>(T eventData, string eventType, CancellationToken cancellationToken = default) where T : class;
    Task PublishAsync(string eventType, object eventData, CancellationToken cancellationToken = default);
}
