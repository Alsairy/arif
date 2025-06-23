using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Common.Security;
using System.Diagnostics;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class OpenTelemetryService
{
    private readonly ILogger<OpenTelemetryService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;
    private readonly ActivitySource _activitySource;
    private readonly Dictionary<string, Activity> _activeActivities;

    public OpenTelemetryService(
        ILogger<OpenTelemetryService> logger,
        IConfiguration configuration,
        IAuditLogger auditLogger)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogger = auditLogger;
        _activitySource = new ActivitySource("Arif.Platform");
        _activeActivities = new Dictionary<string, Activity>();
    }

    public async Task<string> StartActivityAsync(string operationName, ActivityKind kind = ActivityKind.Internal, Dictionary<string, object>? tags = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var activity = _activitySource.StartActivity(operationName, kind);
            
            if (activity == null)
            {
                _logger.LogWarning("Failed to start OpenTelemetry activity for operation: {OperationName}", operationName);
                return string.Empty;
            }

            var activityId = activity.Id ?? Guid.NewGuid().ToString();
            _activeActivities[activityId] = activity;

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity.SetTag(tag.Key, tag.Value?.ToString());
                }
            }

            activity.SetTag("service.name", GetServiceName());
            activity.SetTag("service.version", GetServiceVersion());
            activity.SetTag("tenant.id", GetCurrentTenantId());
            activity.SetTag("user.id", GetCurrentUserId());

            _logger.LogDebug("Started OpenTelemetry activity: {ActivityId} for operation: {OperationName}", activityId, operationName);

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                $"Started OpenTelemetry activity: {operationName} - {activityId}"
            );

            return activityId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting OpenTelemetry activity for operation: {OperationName}", operationName);
            throw;
        }
    }

    public async Task EndActivityAsync(string activityId, bool success = true, Dictionary<string, object>? additionalTags = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_activeActivities.TryGetValue(activityId, out var activity))
            {
                _logger.LogWarning("Attempted to end non-existent OpenTelemetry activity: {ActivityId}", activityId);
                return;
            }

            activity.SetTag("success", success);
            activity.SetTag("end_time", DateTime.UtcNow.ToString("O"));

            if (additionalTags != null)
            {
                foreach (var tag in additionalTags)
                {
                    activity.SetTag(tag.Key, tag.Value?.ToString());
                }
            }

            if (!success)
            {
                activity.SetStatus(ActivityStatusCode.Error, "Operation failed");
            }
            else
            {
                activity.SetStatus(ActivityStatusCode.Ok);
            }

            var operationName = activity.OperationName;
            var duration = activity.Duration;

            activity.Stop();
            _activeActivities.Remove(activityId);

            _logger.LogDebug("Ended OpenTelemetry activity: {ActivityId} for operation: {OperationName} with duration: {Duration}ms", 
                activityId, operationName, duration.TotalMilliseconds);

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                $"Ended OpenTelemetry activity: {operationName} - Duration: {duration.TotalMilliseconds}ms - Success: {success}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending OpenTelemetry activity: {ActivityId}", activityId);
            throw;
        }
    }

    public async Task AddEventAsync(string activityId, string eventName, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_activeActivities.TryGetValue(activityId, out var activity))
            {
                _logger.LogWarning("Attempted to add event to non-existent OpenTelemetry activity: {ActivityId}", activityId);
                return;
            }

            var activityEvent = new ActivityEvent(eventName, DateTime.UtcNow, new ActivityTagsCollection(
                attributes?.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)) ?? 
                Enumerable.Empty<KeyValuePair<string, object?>>()
            ));

            activity.AddEvent(activityEvent);

            _logger.LogDebug("Added event '{EventName}' to OpenTelemetry activity: {ActivityId}", eventName, activityId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding event to OpenTelemetry activity: {ActivityId}", activityId);
            throw;
        }
    }

    public async Task SetTagAsync(string activityId, string key, object value, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_activeActivities.TryGetValue(activityId, out var activity))
            {
                _logger.LogWarning("Attempted to set tag on non-existent OpenTelemetry activity: {ActivityId}", activityId);
                return;
            }

            activity.SetTag(key, value?.ToString());

            _logger.LogDebug("Set tag '{Key}' = '{Value}' on OpenTelemetry activity: {ActivityId}", key, value, activityId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting tag on OpenTelemetry activity: {ActivityId}", activityId);
            throw;
        }
    }

    public async Task RecordExceptionAsync(string activityId, Exception exception, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_activeActivities.TryGetValue(activityId, out var activity))
            {
                _logger.LogWarning("Attempted to record exception on non-existent OpenTelemetry activity: {ActivityId}", activityId);
                return;
            }

            var exceptionAttributes = new Dictionary<string, object>
            {
                ["exception.type"] = exception.GetType().FullName ?? "Unknown",
                ["exception.message"] = exception.Message,
                ["exception.stacktrace"] = exception.StackTrace ?? string.Empty
            };

            await AddEventAsync(activityId, "exception", exceptionAttributes, cancellationToken);

            activity.SetStatus(ActivityStatusCode.Error, exception.Message);

            _logger.LogDebug("Recorded exception on OpenTelemetry activity: {ActivityId} - {ExceptionType}: {ExceptionMessage}", 
                activityId, exception.GetType().Name, exception.Message);

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                $"Recorded exception on OpenTelemetry activity: {activityId} - {exception.GetType().Name}: {exception.Message}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording exception on OpenTelemetry activity: {ActivityId}", activityId);
            throw;
        }
    }

    public async Task<List<ActivityInfo>> GetActiveActivitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activities = _activeActivities.Values.Select(activity => new ActivityInfo
            {
                Id = activity.Id ?? string.Empty,
                OperationName = activity.OperationName,
                StartTime = activity.StartTimeUtc,
                Duration = activity.Duration,
                Status = activity.Status.ToString(),
                Tags = activity.Tags.ToDictionary(tag => tag.Key, tag => tag.Value ?? string.Empty)
            }).ToList();

            _logger.LogDebug("Retrieved {ActivityCount} active OpenTelemetry activities", activities.Count);

            await Task.CompletedTask;
            return activities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active OpenTelemetry activities");
            throw;
        }
    }

    public async Task<TraceContext> GetCurrentTraceContextAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currentActivity = Activity.Current;
            
            if (currentActivity == null)
            {
                return new TraceContext
                {
                    TraceId = string.Empty,
                    SpanId = string.Empty,
                    ParentSpanId = string.Empty,
                    StartTime = DateTime.UtcNow,
                    Baggage = new Dictionary<string, object>()
                };
            }

            var traceContext = new TraceContext
            {
                TraceId = currentActivity.TraceId.ToString(),
                SpanId = currentActivity.SpanId.ToString(),
                ParentSpanId = currentActivity.ParentSpanId.ToString(),
                StartTime = currentActivity.StartTimeUtc,
                Baggage = currentActivity.Baggage.ToDictionary(b => b.Key, b => (object)b.Value)
            };

            await Task.CompletedTask;
            return traceContext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current OpenTelemetry trace context");
            throw;
        }
    }

    private string GetServiceName()
    {
        return _configuration.GetValue<string>("ServiceName") ?? "arif-platform-service";
    }

    private string GetServiceVersion()
    {
        return _configuration.GetValue<string>("ServiceVersion") ?? "1.0.0";
    }

    private string GetCurrentTenantId()
    {
        return "tenant-placeholder";
    }

    private string GetCurrentUserId()
    {
        return "user-placeholder";
    }

    public void Dispose()
    {
        foreach (var activity in _activeActivities.Values)
        {
            activity?.Stop();
        }
        _activeActivities.Clear();
        _activitySource?.Dispose();
    }
}

public class ActivityInfo
{
    public string Id { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, string> Tags { get; set; } = new();
}
