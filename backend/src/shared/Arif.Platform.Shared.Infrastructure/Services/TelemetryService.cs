using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Arif.Platform.Shared.Domain.Interfaces;
using Arif.Platform.Shared.Common.Security;
using DomainLogLevel = Arif.Platform.Shared.Domain.Interfaces.LogLevel;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class TelemetryService : ITelemetryService
{
    private readonly ILogger<TelemetryService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;
    private readonly Dictionary<string, object> _activeTraces;

    public TelemetryService(
        ILogger<TelemetryService> logger,
        IConfiguration configuration,
        IAuditLogger auditLogger)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogger = auditLogger;
        _activeTraces = new Dictionary<string, object>();
    }

    public async Task RecordMetricAsync(string metricName, double value, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
    {
        try
        {
            var metricData = new
            {
                MetricName = metricName,
                Value = value,
                Tags = tags,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Recording metric {MetricName} with value {Value}", metricName, value);

            await Task.Run(() =>
            {
                ProcessMetric(metricData);
            }, cancellationToken);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Recorded metric: {metricName} = {value}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording metric {MetricName}", metricName);
            throw;
        }
    }

    public async Task StartTraceAsync(string operationName, string traceId, Dictionary<string, object> context, CancellationToken cancellationToken = default)
    {
        try
        {
            var traceData = new
            {
                TraceId = traceId,
                OperationName = operationName,
                StartTime = DateTime.UtcNow,
                Context = context,
                Status = "Started"
            };

            _activeTraces[traceId] = traceData;

            _logger.LogInformation("Started trace {TraceId} for operation {OperationName}", traceId, operationName);

            await Task.Run(() =>
            {
                ProcessTraceStart(traceData);
            }, cancellationToken);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Started trace: {traceId} - {operationName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting trace {TraceId}", traceId);
            throw;
        }
    }

    public async Task EndTraceAsync(string traceId, bool success, Dictionary<string, object> result, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_activeTraces.TryGetValue(traceId, out var traceData))
            {
                _logger.LogWarning("Attempted to end non-existent trace {TraceId}", traceId);
                return;
            }

            var endTraceData = new
            {
                TraceId = traceId,
                EndTime = DateTime.UtcNow,
                Success = success,
                Result = result,
                Status = success ? "Completed" : "Failed"
            };

            _activeTraces.Remove(traceId);

            _logger.LogInformation("Ended trace {TraceId} with status {Status}", traceId, success ? "Success" : "Failed");

            await Task.Run(() =>
            {
                ProcessTraceEnd(endTraceData);
            }, cancellationToken);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Ended trace: {traceId} - {(success ? "Success" : "Failed")}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending trace {TraceId}", traceId);
            throw;
        }
    }

    public async Task LogEventAsync(DomainLogLevel level, string message, Dictionary<string, object> properties, CancellationToken cancellationToken = default)
    {
        try
        {
            var logEvent = new
            {
                Level = level,
                Message = message,
                Properties = properties,
                Timestamp = DateTime.UtcNow,
                CorrelationId = await GenerateCorrelationIdAsync(cancellationToken)
            };

            var msLogLevel = ConvertToMicrosoftLogLevel(level);
            _logger.Log(msLogLevel, "Telemetry Event: {Message}", message);

            await Task.Run(() =>
            {
                ProcessLogEvent(logEvent);
            }, cancellationToken);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Logged event: {level} - {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging event with level {Level}", level);
            throw;
        }
    }

    public async Task RecordExceptionAsync(Exception exception, Dictionary<string, object> context, CancellationToken cancellationToken = default)
    {
        try
        {
            var exceptionData = new
            {
                ExceptionType = exception.GetType().Name,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Context = context,
                Timestamp = DateTime.UtcNow,
                CorrelationId = await GenerateCorrelationIdAsync(cancellationToken)
            };

            _logger.LogError(exception, "Exception recorded via telemetry service");

            await Task.Run(() =>
            {
                ProcessException(exceptionData);
            }, cancellationToken);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Recorded exception: {exception.GetType().Name} - {exception.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording exception");
            throw;
        }
    }

    public async Task<string> GenerateCorrelationIdAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var correlationId = Guid.NewGuid().ToString();
            
            await Task.Run(() =>
            {
                SetCorrelationContext(correlationId);
            }, cancellationToken);

            return correlationId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating correlation ID");
            return Guid.NewGuid().ToString(); // Fallback
        }
    }

    private void ProcessMetric(object metricData)
    {
        _logger.LogDebug("Processing metric data: {@MetricData}", metricData);
    }

    private void ProcessTraceStart(object traceData)
    {
        _logger.LogDebug("Processing trace start: {@TraceData}", traceData);
    }

    private void ProcessTraceEnd(object endTraceData)
    {
        _logger.LogDebug("Processing trace end: {@EndTraceData}", endTraceData);
    }

    private void ProcessLogEvent(object logEvent)
    {
        _logger.LogDebug("Processing log event: {@LogEvent}", logEvent);
    }

    private void ProcessException(object exceptionData)
    {
        _logger.LogDebug("Processing exception: {@ExceptionData}", exceptionData);
    }

    private void SetCorrelationContext(string correlationId)
    {
        _logger.LogDebug("Setting correlation context: {CorrelationId}", correlationId);
    }

    private Microsoft.Extensions.Logging.LogLevel ConvertToMicrosoftLogLevel(DomainLogLevel level)
    {
        return level switch
        {
            DomainLogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
            DomainLogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            DomainLogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            DomainLogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            DomainLogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            DomainLogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };
    }
}
