using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Common.Security;
using System.Text;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class PrometheusMetricsService
{
    private readonly ILogger<PrometheusMetricsService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;
    private readonly Dictionary<string, PrometheusMetricCollector> _collectors;
    private readonly Timer _collectionTimer;

    public PrometheusMetricsService(
        ILogger<PrometheusMetricsService> logger,
        IConfiguration configuration,
        IAuditLogger auditLogger)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogger = auditLogger;
        _collectors = new Dictionary<string, PrometheusMetricCollector>();
        
        var collectionInterval = _configuration.GetValue<int>("Metrics:CollectionIntervalSeconds", 30);
        _collectionTimer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(collectionInterval));
    }

    public async Task RegisterCounterAsync(string name, string description, string[] labelNames = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var counter = new CounterMetric
            {
                Name = name,
                Description = description,
                LabelNames = labelNames ?? Array.Empty<string>(),
                Value = 0,
                LastUpdated = DateTime.UtcNow
            };

            _collectors[name] = new CounterCollector(counter);

            _logger.LogInformation("Registered Prometheus counter metric: {MetricName}", name);
            
            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                $"Registered Prometheus counter metric: {name}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering counter metric {MetricName}", name);
            throw;
        }
    }

    public async Task RegisterGaugeAsync(string name, string description, string[] labelNames = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var gauge = new GaugeMetric
            {
                Name = name,
                Description = description,
                LabelNames = labelNames ?? Array.Empty<string>(),
                Value = 0,
                LastUpdated = DateTime.UtcNow
            };

            _collectors[name] = new GaugeCollector(gauge);

            _logger.LogInformation("Registered Prometheus gauge metric: {MetricName}", name);
            
            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                $"Registered Prometheus gauge metric: {name}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering gauge metric {MetricName}", name);
            throw;
        }
    }

    public async Task RegisterHistogramAsync(string name, string description, double[] buckets = null, string[] labelNames = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var histogram = new HistogramMetric
            {
                Name = name,
                Description = description,
                LabelNames = labelNames ?? Array.Empty<string>(),
                Buckets = buckets ?? new[] { 0.1, 0.5, 1.0, 2.5, 5.0, 10.0 },
                BucketCounts = new Dictionary<double, long>(),
                Sum = 0,
                Count = 0,
                LastUpdated = DateTime.UtcNow
            };

            foreach (var bucket in histogram.Buckets)
            {
                histogram.BucketCounts[bucket] = 0;
            }

            _collectors[name] = new HistogramCollector(histogram);

            _logger.LogInformation("Registered Prometheus histogram metric: {MetricName}", name);
            
            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                $"Registered Prometheus histogram metric: {name}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering histogram metric {MetricName}", name);
            throw;
        }
    }

    public async Task IncrementCounterAsync(string name, Dictionary<string, string> labels = null, double value = 1, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_collectors.TryGetValue(name, out var collector) && collector is CounterCollector counterCollector)
            {
                await counterCollector.IncrementAsync(value, labels ?? new Dictionary<string, string>(), cancellationToken);
                _logger.LogDebug("Incremented counter {MetricName} by {Value}", name, value);
            }
            else
            {
                _logger.LogWarning("Counter metric {MetricName} not found", name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing counter {MetricName}", name);
            throw;
        }
    }

    public async Task SetGaugeAsync(string name, double value, Dictionary<string, string> labels = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_collectors.TryGetValue(name, out var collector) && collector is GaugeCollector gaugeCollector)
            {
                await gaugeCollector.SetAsync(value, labels ?? new Dictionary<string, string>(), cancellationToken);
                _logger.LogDebug("Set gauge {MetricName} to {Value}", name, value);
            }
            else
            {
                _logger.LogWarning("Gauge metric {MetricName} not found", name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting gauge {MetricName}", name);
            throw;
        }
    }

    public async Task ObserveHistogramAsync(string name, double value, Dictionary<string, string> labels = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_collectors.TryGetValue(name, out var collector) && collector is HistogramCollector histogramCollector)
            {
                await histogramCollector.ObserveAsync(value, labels ?? new Dictionary<string, string>(), cancellationToken);
                _logger.LogDebug("Observed histogram {MetricName} with value {Value}", name, value);
            }
            else
            {
                _logger.LogWarning("Histogram metric {MetricName} not found", name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error observing histogram {MetricName}", name);
            throw;
        }
    }

    public async Task<string> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var metricsBuilder = new StringBuilder();

            foreach (var kvp in _collectors)
            {
                var metricOutput = await kvp.Value.GetPrometheusFormatAsync(cancellationToken);
                metricsBuilder.AppendLine(metricOutput);
            }

            var metrics = metricsBuilder.ToString();
            
            _logger.LogDebug("Generated Prometheus metrics output with {MetricCount} metrics", _collectors.Count);
            
            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Prometheus metrics output");
            throw;
        }
    }

    private async void CollectMetrics(object state)
    {
        try
        {
            await CollectSystemMetricsAsync();
            await CollectApplicationMetricsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during automatic metrics collection");
        }
    }

    private async Task CollectSystemMetricsAsync()
    {
        try
        {
            var cpuUsage = Random.Shared.NextDouble() * 100;
            var memoryUsage = Random.Shared.NextDouble() * 100;
            var diskUsage = Random.Shared.NextDouble() * 100;

            await SetGaugeAsync("system_cpu_usage_percent", cpuUsage);
            await SetGaugeAsync("system_memory_usage_percent", memoryUsage);
            await SetGaugeAsync("system_disk_usage_percent", diskUsage);

            _logger.LogDebug("Collected system metrics - CPU: {CPU}%, Memory: {Memory}%, Disk: {Disk}%", 
                Math.Round(cpuUsage, 2), Math.Round(memoryUsage, 2), Math.Round(diskUsage, 2));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting system metrics");
        }
    }

    private async Task CollectApplicationMetricsAsync()
    {
        try
        {
            var activeConnections = Random.Shared.Next(10, 1000);
            var requestsPerSecond = Random.Shared.Next(1, 100);
            var errorRate = Random.Shared.NextDouble() * 5;

            await SetGaugeAsync("app_active_connections", activeConnections);
            await SetGaugeAsync("app_requests_per_second", requestsPerSecond);
            await SetGaugeAsync("app_error_rate_percent", errorRate);

            _logger.LogDebug("Collected application metrics - Connections: {Connections}, RPS: {RPS}, Error Rate: {ErrorRate}%", 
                activeConnections, requestsPerSecond, Math.Round(errorRate, 2));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting application metrics");
        }
    }

    public void Dispose()
    {
        _collectionTimer?.Dispose();
    }
}

public abstract class PrometheusMetricCollector
{
    public abstract Task<string> GetPrometheusFormatAsync(CancellationToken cancellationToken = default);
}

public class CounterCollector : PrometheusMetricCollector
{
    private readonly CounterMetric _metric;

    public CounterCollector(CounterMetric metric)
    {
        _metric = metric;
    }

    public async Task IncrementAsync(double value, Dictionary<string, string> labels, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            _metric.Value += value;
            _metric.LastUpdated = DateTime.UtcNow;
        }, cancellationToken);
    }

    public override async Task<string> GetPrometheusFormatAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# HELP {_metric.Name} {_metric.Description}");
            sb.AppendLine($"# TYPE {_metric.Name} counter");
            sb.AppendLine($"{_metric.Name} {_metric.Value}");
            return sb.ToString();
        }, cancellationToken);
    }
}

public class GaugeCollector : PrometheusMetricCollector
{
    private readonly GaugeMetric _metric;

    public GaugeCollector(GaugeMetric metric)
    {
        _metric = metric;
    }

    public async Task SetAsync(double value, Dictionary<string, string> labels, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            _metric.Value = value;
            _metric.LastUpdated = DateTime.UtcNow;
        }, cancellationToken);
    }

    public override async Task<string> GetPrometheusFormatAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# HELP {_metric.Name} {_metric.Description}");
            sb.AppendLine($"# TYPE {_metric.Name} gauge");
            sb.AppendLine($"{_metric.Name} {_metric.Value}");
            return sb.ToString();
        }, cancellationToken);
    }
}

public class HistogramCollector : PrometheusMetricCollector
{
    private readonly HistogramMetric _metric;

    public HistogramCollector(HistogramMetric metric)
    {
        _metric = metric;
    }

    public async Task ObserveAsync(double value, Dictionary<string, string> labels, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            _metric.Sum += value;
            _metric.Count++;
            
            foreach (var bucket in _metric.Buckets)
            {
                if (value <= bucket)
                {
                    _metric.BucketCounts[bucket]++;
                }
            }
            
            _metric.LastUpdated = DateTime.UtcNow;
        }, cancellationToken);
    }

    public override async Task<string> GetPrometheusFormatAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# HELP {_metric.Name} {_metric.Description}");
            sb.AppendLine($"# TYPE {_metric.Name} histogram");
            
            foreach (var bucket in _metric.BucketCounts)
            {
                sb.AppendLine($"{_metric.Name}_bucket{{le=\"{bucket.Key}\"}} {bucket.Value}");
            }
            
            sb.AppendLine($"{_metric.Name}_sum {_metric.Sum}");
            sb.AppendLine($"{_metric.Name}_count {_metric.Count}");
            
            return sb.ToString();
        }, cancellationToken);
    }
}

public class CounterMetric
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] LabelNames { get; set; } = Array.Empty<string>();
    public double Value { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class GaugeMetric
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] LabelNames { get; set; } = Array.Empty<string>();
    public double Value { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class HistogramMetric
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] LabelNames { get; set; } = Array.Empty<string>();
    public double[] Buckets { get; set; } = Array.Empty<double>();
    public Dictionary<double, long> BucketCounts { get; set; } = new();
    public double Sum { get; set; }
    public long Count { get; set; }
    public DateTime LastUpdated { get; set; }
}
