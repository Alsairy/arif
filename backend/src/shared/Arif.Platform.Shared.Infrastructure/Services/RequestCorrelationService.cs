using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Domain.Interfaces;
using Arif.Platform.Shared.Common.Security;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class RequestCorrelationService : IRequestCorrelationService
{
    private readonly ILogger<RequestCorrelationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;
    private readonly Dictionary<string, List<RequestTrace>> _requestTraces;
    private readonly Dictionary<string, List<ServiceCall>> _serviceCalls;

    public RequestCorrelationService(
        ILogger<RequestCorrelationService> logger,
        IConfiguration configuration,
        IAuditLogger auditLogger)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogger = auditLogger;
        _requestTraces = new Dictionary<string, List<RequestTrace>>();
        _serviceCalls = new Dictionary<string, List<ServiceCall>>();
    }

    public async Task<string> GenerateCorrelationIdAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var correlationId = $"req_{DateTime.UtcNow:yyyyMMdd}_{Guid.NewGuid():N}";
            
            _requestTraces[correlationId] = new List<RequestTrace>();
            _serviceCalls[correlationId] = new List<ServiceCall>();

            _logger.LogDebug("Generated correlation ID: {CorrelationId}", correlationId);

            await Task.CompletedTask;
            return correlationId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating correlation ID");
            throw;
        }
    }

    public async Task<RequestTrace> StartRequestTraceAsync(string correlationId, string serviceName, string operationName, CancellationToken cancellationToken = default)
    {
        try
        {
            var trace = new RequestTrace
            {
                Id = Guid.NewGuid(),
                CorrelationId = correlationId,
                ServiceName = serviceName,
                OperationName = operationName,
                StartTime = DateTime.UtcNow,
                Success = false,
                Metadata = new Dictionary<string, object>
                {
                    ["thread_id"] = Environment.CurrentManagedThreadId,
                    ["machine_name"] = Environment.MachineName,
                    ["process_id"] = Environment.ProcessId
                },
                ServiceCalls = new List<ServiceCall>()
            };

            if (!_requestTraces.ContainsKey(correlationId))
            {
                _requestTraces[correlationId] = new List<RequestTrace>();
            }

            _requestTraces[correlationId].Add(trace);

            await _auditLogger.LogUserActionAsync(
                "request_trace_started",
                Guid.Empty, // TODO: Get current user ID
                Guid.Empty, // TODO: Get current tenant ID
                $"Started request trace: {serviceName}.{operationName} - Correlation: {correlationId}"
            );

            _logger.LogDebug("Started request trace: {ServiceName}.{OperationName} - Correlation: {CorrelationId} - Trace: {TraceId}", 
                serviceName, operationName, correlationId, trace.Id);

            return trace;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting request trace for correlation: {CorrelationId}", correlationId);
            throw;
        }
    }

    public async Task EndRequestTraceAsync(string traceId, bool success, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default)
    {
        try
        {
            RequestTrace? targetTrace = null;
            string? correlationId = null;

            foreach (var kvp in _requestTraces)
            {
                var trace = kvp.Value.FirstOrDefault(t => t.Id.ToString() == traceId);
                if (trace != null)
                {
                    targetTrace = trace;
                    correlationId = kvp.Key;
                    break;
                }
            }

            if (targetTrace == null)
            {
                _logger.LogWarning("Request trace not found: {TraceId}", traceId);
                return;
            }

            targetTrace.EndTime = DateTime.UtcNow;
            targetTrace.Duration = targetTrace.EndTime.Value - targetTrace.StartTime;
            targetTrace.Success = success;

            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    targetTrace.Metadata[kvp.Key] = kvp.Value;
                }
            }

            await _auditLogger.LogUserActionAsync(
                "request_trace_ended",
                Guid.Empty, // TODO: Get current user ID
                Guid.Empty, // TODO: Get current tenant ID
                $"Ended request trace: {targetTrace.ServiceName}.{targetTrace.OperationName} - Duration: {targetTrace.Duration?.TotalMilliseconds}ms - Success: {success}"
            );

            _logger.LogDebug("Ended request trace: {ServiceName}.{OperationName} - Duration: {Duration}ms - Success: {Success}", 
                targetTrace.ServiceName, targetTrace.OperationName, targetTrace.Duration?.TotalMilliseconds, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending request trace: {TraceId}", traceId);
            throw;
        }
    }

    public async Task<List<RequestTrace>> GetRequestTraceAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_requestTraces.TryGetValue(correlationId, out var traces))
            {
                return new List<RequestTrace>();
            }

            var result = traces
                .OrderBy(trace => trace.StartTime)
                .ToList();

            await Task.CompletedTask;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving request trace: {CorrelationId}", correlationId);
            throw;
        }
    }

    public async Task<List<ServiceCall>> GetServiceCallChainAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_serviceCalls.TryGetValue(correlationId, out var calls))
            {
                return new List<ServiceCall>();
            }

            var result = calls
                .OrderBy(call => call.StartTime)
                .ToList();

            await Task.CompletedTask;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service call chain: {CorrelationId}", correlationId);
            throw;
        }
    }

    public async Task LogServiceCallAsync(string correlationId, string fromService, string toService, string operation, TimeSpan duration, bool success, CancellationToken cancellationToken = default)
    {
        try
        {
            var serviceCall = new ServiceCall
            {
                Id = Guid.NewGuid(),
                CorrelationId = correlationId,
                FromService = fromService,
                ToService = toService,
                Operation = operation,
                StartTime = DateTime.UtcNow - duration,
                Duration = duration,
                Success = success,
                Metadata = new Dictionary<string, object>
                {
                    ["logged_at"] = DateTime.UtcNow,
                    ["thread_id"] = Environment.CurrentManagedThreadId
                }
            };

            if (!success)
            {
                serviceCall.ErrorMessage = "Service call failed";
            }

            if (!_serviceCalls.ContainsKey(correlationId))
            {
                _serviceCalls[correlationId] = new List<ServiceCall>();
            }

            _serviceCalls[correlationId].Add(serviceCall);

            await _auditLogger.LogUserActionAsync(
                "service_call_logged",
                Guid.Empty, // TODO: Get current user ID
                Guid.Empty, // TODO: Get current tenant ID
                $"Service call logged: {fromService} -> {toService}.{operation} - Duration: {duration.TotalMilliseconds}ms - Success: {success}"
            );

            _logger.LogDebug("Service call logged: {FromService} -> {ToService}.{Operation} - Duration: {Duration}ms - Success: {Success}", 
                fromService, toService, operation, duration.TotalMilliseconds, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging service call for correlation: {CorrelationId}", correlationId);
            throw;
        }
    }
}
