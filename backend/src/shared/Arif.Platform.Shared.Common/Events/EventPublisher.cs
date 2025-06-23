using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Arif.Platform.Shared.Common.Events;

public class EventPublisher : IEventPublisher
{
    private readonly ILogger<EventPublisher> _logger;
    private readonly Dictionary<string, List<Func<object, Task>>> _handlers;

    public EventPublisher(ILogger<EventPublisher> logger)
    {
        _logger = logger;
        _handlers = new Dictionary<string, List<Func<object, Task>>>();
    }

    public async Task PublishAsync<T>(T eventData, string eventType, CancellationToken cancellationToken = default) where T : class
    {
        await PublishAsync(eventType, eventData, cancellationToken);
    }

    public async Task PublishAsync(string eventType, object eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Publishing event {EventType} with data: {EventData}", 
                eventType, JsonSerializer.Serialize(eventData));

            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                var tasks = handlers.Select(handler => handler(eventData));
                await Task.WhenAll(tasks);
            }

            _logger.LogInformation("Successfully published event {EventType}", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType}", eventType);
            throw;
        }
    }

    public void Subscribe<T>(string eventType, Func<T, Task> handler) where T : class
    {
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Func<object, Task>>();
        }

        _handlers[eventType].Add(async (data) =>
        {
            if (data is T typedData)
            {
                await handler(typedData);
            }
        });
    }

    public void Subscribe(string eventType, Func<object, Task> handler)
    {
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Func<object, Task>>();
        }

        _handlers[eventType].Add(handler);
    }
}
