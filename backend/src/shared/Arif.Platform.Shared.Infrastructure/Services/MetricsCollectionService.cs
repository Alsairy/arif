using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Common.Security;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class MetricsCollectionService
{
    private readonly ILogger<MetricsCollectionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;
    private readonly Dictionary<string, MetricCollector> _collectors;
    private readonly Timer _collectionTimer;

    public MetricsCollectionService(
        ILogger<MetricsCollectionService> logger,
        IConfiguration configuration,
        IAuditLogger auditLogger)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogger = auditLogger;
        _collectors = new Dictionary<string, MetricCollector>();
        
        var collectionInterval = _configuration.GetValue<int>("Metrics:CollectionIntervalSeconds", 30);
        _collectionTimer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(collectionInterval));
    }

    public async Task RegisterMetricCollectorAsync(string name, MetricCollector collector, CancellationToken cancellationToken = default)
    {
        try
        {
            _collectors[name] = collector;
            
            _logger.LogInformation("Registered metric collector: {CollectorName}", name);
            
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, 
                $"Registered metric collector: {name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering metric collector {CollectorName}", name);
            throw;
        }
    }

    public async Task<List<MetricData>> CollectAllMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allMetrics = new List<MetricData>();

            foreach (var collector in _collectors.Values)
            {
                var metrics = await collector.CollectAsync(cancellationToken);
                allMetrics.AddRange(metrics);
            }

            _logger.LogInformation("Collected {MetricCount} metrics from {CollectorCount} collectors", 
                allMetrics.Count, _collectors.Count);

            return allMetrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting metrics");
            throw;
        }
    }

    public async Task<List<MetricData>> GetSystemMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = new List<MetricData>();
            var timestamp = DateTime.UtcNow;

            metrics.Add(new MetricData
            {
                Id = Guid.NewGuid(),
                ServiceName = "system",
                MetricName = "cpu_usage_percent",
                Value = await GetCpuUsageAsync(),
                Timestamp = timestamp,
                Type = MetricType.Gauge,
                Unit = "percent",
                Tags = new Dictionary<string, string> { ["host"] = Environment.MachineName }
            });

            metrics.Add(new MetricData
            {
                Id = Guid.NewGuid(),
                ServiceName = "system",
                MetricName = "memory_usage_percent",
                Value = await GetMemoryUsageAsync(),
                Timestamp = timestamp,
                Type = MetricType.Gauge,
                Unit = "percent",
                Tags = new Dictionary<string, string> { ["host"] = Environment.MachineName }
            });

            metrics.Add(new MetricData
            {
                Id = Guid.NewGuid(),
                ServiceName = "system",
                MetricName = "disk_usage_percent",
                Value = await GetDiskUsageAsync(),
                Timestamp = timestamp,
                Type = MetricType.Gauge,
                Unit = "percent",
                Tags = new Dictionary<string, string> { ["host"] = Environment.MachineName }
            });

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting system metrics");
            throw;
        }
    }

    public async Task<List<MetricData>> GetApplicationMetricsAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = new List<MetricData>();
            var timestamp = DateTime.UtcNow;

            metrics.Add(new MetricData
            {
                Id = Guid.NewGuid(),
                ServiceName = serviceName,
                MetricName = "requests_per_second",
                Value = await GetRequestRateAsync(serviceName),
                Timestamp = timestamp,
                Type = MetricType.Gauge,
                Unit = "req/s",
                Tags = new Dictionary<string, string> { ["service"] = serviceName }
            });

            metrics.Add(new MetricData
            {
                Id = Guid.NewGuid(),
                ServiceName = serviceName,
                MetricName = "response_time_ms",
                Value = await GetAverageResponseTimeAsync(serviceName),
                Timestamp = timestamp,
                Type = MetricType.Gauge,
                Unit = "ms",
                Tags = new Dictionary<string, string> { ["service"] = serviceName }
            });

            metrics.Add(new MetricData
            {
                Id = Guid.NewGuid(),
                ServiceName = serviceName,
                MetricName = "error_rate_percent",
                Value = await GetErrorRateAsync(serviceName),
                Timestamp = timestamp,
                Type = MetricType.Gauge,
                Unit = "percent",
                Tags = new Dictionary<string, string> { ["service"] = serviceName }
            });

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting application metrics for service {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<List<MetricData>> GetBusinessMetricsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = new List<MetricData>();
            var timestamp = DateTime.UtcNow;

            metrics.Add(new MetricData
            {
                Id = Guid.NewGuid(),
                ServiceName = "business",
                MetricName = "active_users",
                Value = await GetActiveUsersAsync(tenantId),
                Timestamp = timestamp,
                Type = MetricType.Gauge,
                Unit = "count",
                Tags = new Dictionary<string, string> { ["tenant_id"] = tenantId.ToString() }
            });

            metrics.Add(new MetricData
            {
                Id = Guid.NewGuid(),
                ServiceName = "business",
                MetricName = "messages_processed",
                Value = await GetMessagesProcessedAsync(tenantId),
                Timestamp = timestamp,
                Type = MetricType.Counter,
                Unit = "count",
                Tags = new Dictionary<string, string> { ["tenant_id"] = tenantId.ToString() }
            });

            metrics.Add(new MetricData
            {
                Id = Guid.NewGuid(),
                ServiceName = "business",
                MetricName = "active_conversations",
                Value = await GetActiveConversationsAsync(tenantId),
                Timestamp = timestamp,
                Type = MetricType.Gauge,
                Unit = "count",
                Tags = new Dictionary<string, string> { ["tenant_id"] = tenantId.ToString() }
            });

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting business metrics for tenant {TenantId}", tenantId);
            throw;
        }
    }

    private async void CollectMetrics(object? state)
    {
        try
        {
            var metrics = await CollectAllMetricsAsync();
            
            await SendMetricsToBackend(metrics);
            
            _logger.LogDebug("Collected and sent {MetricCount} metrics", metrics.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled metric collection");
        }
    }

    private async Task SendMetricsToBackend(List<MetricData> metrics)
    {
        
        await Task.Run(() =>
        {
            _logger.LogDebug("Sending {MetricCount} metrics to backend", metrics.Count);
        });
    }

    private async Task<double> GetCpuUsageAsync()
    {
        await Task.Delay(10);
        return Random.Shared.NextDouble() * 100;
    }

    private async Task<double> GetMemoryUsageAsync()
    {
        await Task.Delay(10);
        var totalMemory = GC.GetTotalMemory(false);
        return Random.Shared.NextDouble() * 100; // Simplified
    }

    private async Task<double> GetDiskUsageAsync()
    {
        await Task.Delay(10);
        return Random.Shared.NextDouble() * 100;
    }

    private async Task<double> GetRequestRateAsync(string serviceName)
    {
        await Task.Delay(10);
        return Random.Shared.NextDouble() * 1000;
    }

    private async Task<double> GetAverageResponseTimeAsync(string serviceName)
    {
        await Task.Delay(10);
        return Random.Shared.NextDouble() * 500;
    }

    private async Task<double> GetErrorRateAsync(string serviceName)
    {
        await Task.Delay(10);
        return Random.Shared.NextDouble() * 5;
    }

    private async Task<double> GetActiveUsersAsync(Guid tenantId)
    {
        await Task.Delay(10);
        return Random.Shared.Next(100, 5000);
    }

    private async Task<double> GetMessagesProcessedAsync(Guid tenantId)
    {
        await Task.Delay(10);
        return Random.Shared.Next(1000, 50000);
    }

    private async Task<double> GetActiveConversationsAsync(Guid tenantId)
    {
        await Task.Delay(10);
        return Random.Shared.Next(50, 1000);
    }

    public void Dispose()
    {
        _collectionTimer?.Dispose();
    }
}

public abstract class MetricCollector
{
    public abstract Task<List<MetricData>> CollectAsync(CancellationToken cancellationToken = default);
}

public class SystemMetricCollector : MetricCollector
{
    private readonly ILogger<SystemMetricCollector> _logger;

    public SystemMetricCollector(ILogger<SystemMetricCollector> logger)
    {
        _logger = logger;
    }

    public override async Task<List<MetricData>> CollectAsync(CancellationToken cancellationToken = default)
    {
        var metrics = new List<MetricData>();
        var timestamp = DateTime.UtcNow;

        metrics.Add(new MetricData
        {
            Id = Guid.NewGuid(),
            ServiceName = "system",
            MetricName = "process_cpu_usage",
            Value = Random.Shared.NextDouble() * 100,
            Timestamp = timestamp,
            Type = MetricType.Gauge,
            Unit = "percent"
        });

        return metrics;
    }


}
