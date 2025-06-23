using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Domain.Interfaces;
using Arif.Platform.Shared.Common.Security;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class DistributedTracingService : IDistributedTracingService
{
    private readonly ILogger<DistributedTracingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;
    private readonly Dictionary<string, TraceContext> _activeTraces;

    public DistributedTracingService(
        ILogger<DistributedTracingService> logger,
        IConfiguration configuration,
        IAuditLogger auditLogger)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogger = auditLogger;
        _activeTraces = new Dictionary<string, TraceContext>();
    }

    public async Task<TraceContext> StartTraceAsync(string operationName, TraceContext? parentContext = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var traceContext = new TraceContext
            {
                TraceId = parentContext?.TraceId ?? Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ParentSpanId = parentContext?.SpanId ?? string.Empty,
                StartTime = DateTime.UtcNow,
                Baggage = parentContext?.Baggage ?? new Dictionary<string, object>()
            };

            traceContext.Baggage["operation.name"] = operationName;
            traceContext.Baggage["service.name"] = GetServiceName();
            traceContext.Baggage["service.version"] = GetServiceVersion();

            _activeTraces[traceContext.SpanId] = traceContext;

            _logger.LogInformation("Started distributed trace {TraceId} with span {SpanId} for operation {OperationName}", 
                traceContext.TraceId, traceContext.SpanId, operationName);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, 
                $"Started trace: {traceContext.TraceId} - {operationName}");

            return traceContext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting distributed trace for operation {OperationName}", operationName);
            throw;
        }
    }

    public async Task EndTraceAsync(TraceContext context, bool success, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_activeTraces.ContainsKey(context.SpanId))
            {
                _logger.LogWarning("Attempted to end non-existent trace span {SpanId}", context.SpanId);
                return;
            }

            var endTime = DateTime.UtcNow;
            var duration = endTime - context.StartTime;

            var traceSpan = new TraceSpan
            {
                SpanId = context.SpanId,
                ParentSpanId = context.ParentSpanId,
                OperationName = context.Baggage.GetValueOrDefault("operation.name", "unknown").ToString() ?? "unknown",
                ServiceName = context.Baggage.GetValueOrDefault("service.name", "unknown").ToString() ?? "unknown",
                StartTime = context.StartTime,
                EndTime = endTime,
                Duration = duration,
                Tags = new Dictionary<string, object>(context.Baggage)
            };

            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    traceSpan.Tags[kvp.Key] = kvp.Value;
                }
            }

            traceSpan.Tags["success"] = success;
            traceSpan.Tags["duration_ms"] = duration.TotalMilliseconds;

            _activeTraces.Remove(context.SpanId);

            _logger.LogInformation("Ended distributed trace {TraceId} span {SpanId} with duration {Duration}ms, success: {Success}", 
                context.TraceId, context.SpanId, duration.TotalMilliseconds, success);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, 
                $"Ended trace: {context.TraceId} - Duration: {duration.TotalMilliseconds}ms - Success: {success}");

            await SendTraceSpanAsync(traceSpan, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending distributed trace for span {SpanId}", context.SpanId);
            throw;
        }
    }

    public async Task<List<TraceSpan>> GetTraceSpansAsync(string traceId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, 
                $"Retrieving trace spans for: {traceId}");

            var spans = await QueryTraceSpansFromBackend(traceId, cancellationToken);

            _logger.LogInformation("Retrieved {SpanCount} spans for trace {TraceId}", spans.Count, traceId);

            return spans;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trace spans for trace {TraceId}", traceId);
            throw;
        }
    }

    public async Task<TraceAnalytics> AnalyzeTraceAsync(string traceId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, 
                $"Analyzing trace: {traceId}");

            var spans = await GetTraceSpansAsync(traceId, cancellationToken);

            var analytics = new TraceAnalytics
            {
                TraceId = traceId,
                TotalDuration = CalculateTotalDuration(spans),
                SpanCount = spans.Count,
                ServiceCount = spans.Select(s => s.ServiceName).Distinct().Count(),
                ServiceLatencies = CalculateServiceLatencies(spans),
                CriticalPaths = CalculateCriticalPaths(spans),
                Errors = ExtractTraceErrors(spans)
            };

            _logger.LogInformation("Analyzed trace {TraceId}: {SpanCount} spans, {ServiceCount} services, {Duration}ms total", 
                traceId, analytics.SpanCount, analytics.ServiceCount, analytics.TotalDuration.TotalMilliseconds);

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing trace {TraceId}", traceId);
            throw;
        }
    }

    public async Task<List<ServiceDependency>> GetServiceDependenciesAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, 
                $"Retrieving dependencies for service: {serviceName}");

            var dependencies = await AnalyzeServiceDependencies(serviceName, cancellationToken);

            _logger.LogInformation("Found {DependencyCount} dependencies for service {ServiceName}", 
                dependencies.Count, serviceName);

            return dependencies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service dependencies for {ServiceName}", serviceName);
            throw;
        }
    }

    private async Task SendTraceSpanAsync(TraceSpan span, CancellationToken cancellationToken)
    {
        
        await Task.Run(() =>
        {
            _logger.LogDebug("Sending trace span to backend: {@TraceSpan}", span);
        }, cancellationToken);
    }

    private async Task<List<TraceSpan>> QueryTraceSpansFromBackend(string traceId, CancellationToken cancellationToken)
    {
        
        await Task.Delay(100, cancellationToken); // Simulate backend call

        var spans = new List<TraceSpan>();
        var services = new[] { "api-gateway", "auth-service", "chatbot-runtime", "ai-orchestration" };
        var baseTime = DateTime.UtcNow.AddMinutes(-5);

        for (int i = 0; i < services.Length; i++)
        {
            spans.Add(new TraceSpan
            {
                SpanId = Guid.NewGuid().ToString(),
                ParentSpanId = i == 0 ? string.Empty : spans[i - 1].SpanId,
                OperationName = $"{services[i]}.process",
                ServiceName = services[i],
                StartTime = baseTime.AddSeconds(i * 10),
                EndTime = baseTime.AddSeconds(i * 10 + Random.Shared.Next(5, 15)),
                Duration = TimeSpan.FromSeconds(Random.Shared.Next(5, 15)),
                Tags = new Dictionary<string, object>
                {
                    ["service.version"] = "1.0.0",
                    ["success"] = true,
                    ["http.status_code"] = 200
                }
            });
        }

        return spans;
    }

    private TimeSpan CalculateTotalDuration(List<TraceSpan> spans)
    {
        if (!spans.Any()) return TimeSpan.Zero;

        var minStart = spans.Min(s => s.StartTime);
        var maxEnd = spans.Max(s => s.EndTime);
        return maxEnd - minStart;
    }

    private List<ServiceLatency> CalculateServiceLatencies(List<TraceSpan> spans)
    {
        return spans
            .GroupBy(s => s.ServiceName)
            .Select(g => new ServiceLatency
            {
                ServiceName = g.Key,
                TotalTime = TimeSpan.FromMilliseconds(g.Sum(s => s.Duration.TotalMilliseconds)),
                SpanCount = g.Count(),
                Percentage = g.Sum(s => s.Duration.TotalMilliseconds) / spans.Sum(s => s.Duration.TotalMilliseconds) * 100
            })
            .OrderByDescending(sl => sl.TotalTime)
            .ToList();
    }

    private List<CriticalPath> CalculateCriticalPaths(List<TraceSpan> spans)
    {
        var criticalPath = new CriticalPath
        {
            SpanIds = spans.OrderByDescending(s => s.Duration).Take(3).Select(s => s.SpanId).ToList(),
            Duration = TimeSpan.FromMilliseconds(spans.OrderByDescending(s => s.Duration).Take(3).Sum(s => s.Duration.TotalMilliseconds)),
            PercentageOfTotal = spans.OrderByDescending(s => s.Duration).Take(3).Sum(s => s.Duration.TotalMilliseconds) / 
                              spans.Sum(s => s.Duration.TotalMilliseconds) * 100
        };

        return new List<CriticalPath> { criticalPath };
    }

    private List<TraceError> ExtractTraceErrors(List<TraceSpan> spans)
    {
        return spans
            .Where(s => s.Tags.ContainsKey("error") || s.Tags.ContainsKey("exception") || 
                       (s.Tags.ContainsKey("success") && !(bool)s.Tags["success"]))
            .Select(s => new TraceError
            {
                SpanId = s.SpanId,
                ServiceName = s.ServiceName,
                ErrorMessage = s.Tags.GetValueOrDefault("error.message", "Unknown error").ToString() ?? "Unknown error",
                Timestamp = s.StartTime
            })
            .ToList();
    }

    private async Task<List<ServiceDependency>> AnalyzeServiceDependencies(string serviceName, CancellationToken cancellationToken)
    {
        
        await Task.Delay(100, cancellationToken); // Simulate analysis

        var dependencies = new List<ServiceDependency>();

        if (serviceName == "api-gateway")
        {
            dependencies.AddRange(new[]
            {
                new ServiceDependency
                {
                    ServiceName = serviceName,
                    DependentService = "auth-service",
                    CallCount = Random.Shared.Next(100, 1000),
                    AverageLatency = TimeSpan.FromMilliseconds(Random.Shared.Next(50, 200)),
                    ErrorRate = Random.Shared.NextDouble() * 5,
                    Type = DependencyType.ExternalApi
                },
                new ServiceDependency
                {
                    ServiceName = serviceName,
                    DependentService = "chatbot-runtime",
                    CallCount = Random.Shared.Next(500, 2000),
                    AverageLatency = TimeSpan.FromMilliseconds(Random.Shared.Next(100, 300)),
                    ErrorRate = Random.Shared.NextDouble() * 3,
                    Type = DependencyType.ExternalApi
                }
            });
        }

        return dependencies;
    }

    private string GetServiceName()
    {
        return _configuration.GetValue<string>("ServiceName") ?? "unknown-service";
    }

    private string GetServiceVersion()
    {
        return _configuration.GetValue<string>("ServiceVersion") ?? "1.0.0";
    }
}
