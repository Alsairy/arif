using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Domain.Interfaces;
using Arif.Platform.Shared.Common.Security;
using DomainLogLevel = Arif.Platform.Shared.Domain.Interfaces.LogLevel;
using EntityLogLevel = Arif.Platform.Shared.Domain.Entities.LogLevel;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class ObservabilityService : IObservabilityService
{
    private readonly ILogger<ObservabilityService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;

    public ObservabilityService(
        ILogger<ObservabilityService> logger,
        IConfiguration configuration,
        IAuditLogger auditLogger)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogger = auditLogger;
    }

    public async Task<List<MetricData>> GetMetricsAsync(string serviceName, TimeSpan timeRange, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Retrieving metrics for service: {serviceName}");

            var endTime = DateTime.UtcNow;
            var startTime = endTime.Subtract(timeRange);

            var metrics = new List<MetricData>();

            metrics.AddRange(GeneratePerformanceMetrics(serviceName, startTime, endTime));
            metrics.AddRange(GenerateResourceMetrics(serviceName, startTime, endTime));
            metrics.AddRange(GenerateBusinessMetrics(serviceName, startTime, endTime));

            return metrics.OrderBy(m => m.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics for service {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<List<TraceData>> GetTracesAsync(string traceId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Retrieving traces for ID: {traceId}");

            var traces = new List<TraceData>();

            var rootTrace = new TraceData
            {
                Id = Guid.NewGuid(),
                TraceId = traceId,
                SpanId = Guid.NewGuid().ToString(),
                OperationName = "HTTP Request",
                ServiceName = "api-gateway",
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                EndTime = DateTime.UtcNow.AddMinutes(-4),
                Duration = TimeSpan.FromMinutes(1),
                Status = TraceStatus.Ok,
                Tags = new Dictionary<string, object>
                {
                    ["http.method"] = "POST",
                    ["http.url"] = "/api/chat/message",
                    ["http.status_code"] = 200
                }
            };

            traces.Add(rootTrace);

            var childTraces = GenerateChildTraces(traceId, rootTrace.SpanId);
            traces.AddRange(childTraces);

            return traces.OrderBy(t => t.StartTime).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving traces for ID {TraceId}", traceId);
            throw;
        }
    }

    public async Task<List<LogEntry>> GetLogsAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Retrieving logs for service: {query.ServiceName}");

            var logs = new List<LogEntry>();

            var sampleLogs = GenerateSampleLogs(query.ServiceName, query.StartTime, query.EndTime);
            
            logs.AddRange(sampleLogs.Where(log => 
                (int)log.Level >= (int)query.MinLevel &&
                (string.IsNullOrEmpty(query.SearchText) || log.Message.Contains(query.SearchText, StringComparison.OrdinalIgnoreCase))
            ).Take(query.MaxResults));

            return logs.OrderByDescending(l => l.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for service {ServiceName}", query.ServiceName);
            throw;
        }
    }

    public async Task<HealthCheckResult> GetServiceHealthAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Checking health for service: {serviceName}");

            var healthCheck = new HealthCheckResult
            {
                Id = Guid.NewGuid(),
                ServiceName = serviceName,
                Status = HealthStatus.Healthy,
                CheckTime = DateTime.UtcNow,
                ResponseTime = TimeSpan.FromMilliseconds(Random.Shared.Next(50, 200)),
                Description = $"Service {serviceName} is operating normally",
                Data = new Dictionary<string, object>
                {
                    ["version"] = "1.0.0",
                    ["uptime"] = TimeSpan.FromHours(Random.Shared.Next(1, 720)).ToString(),
                    ["memory_usage"] = $"{Random.Shared.Next(30, 80)}%",
                    ["cpu_usage"] = $"{Random.Shared.Next(10, 60)}%"
                },
                Dependencies = GenerateHealthCheckDependencies(serviceName)
            };

            return healthCheck;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health for service {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<List<AlertRule>> GetActiveAlertsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Retrieving active alerts for tenant: {tenantId}");

            var alerts = new List<AlertRule>
            {
                new AlertRule
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId.ToString(),
                    Name = "High Response Time",
                    Description = "Alert when average response time exceeds 2 seconds",
                    MetricName = "http_request_duration",
                    Condition = AlertCondition.GreaterThan,
                    Threshold = 2000,
                    Severity = AlertSeverity.High,
                    IsEnabled = true,
                    LastTriggered = DateTime.UtcNow.AddHours(-2),
                    TriggerCount = 3
                },
                new AlertRule
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId.ToString(),
                    Name = "High Error Rate",
                    Description = "Alert when error rate exceeds 5%",
                    MetricName = "error_rate",
                    Condition = AlertCondition.GreaterThan,
                    Threshold = 0.05,
                    Severity = AlertSeverity.Critical,
                    IsEnabled = true,
                    LastTriggered = null,
                    TriggerCount = 0
                }
            };

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active alerts for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> CreateAlertRuleAsync(AlertRule alertRule, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Creating alert rule: {alertRule.Name}");

            alertRule.Id = Guid.NewGuid();
            alertRule.CreatedAt = DateTime.UtcNow;

            _logger.LogInformation("Created alert rule {AlertRuleName} for tenant {TenantId}", alertRule.Name, alertRule.TenantId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alert rule {AlertRuleName}", alertRule.Name);
            throw;
        }
    }

    public async Task<DashboardData> GetDashboardDataAsync(string dashboardId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Retrieving dashboard data: {dashboardId}");

            var dashboard = new DashboardData
            {
                Id = Guid.NewGuid(),
                DashboardId = dashboardId,
                TenantId = tenantId,
                Title = "System Overview Dashboard",
                LastUpdated = DateTime.UtcNow,
                // Widgets = GenerateDashboardWidgets(), // TODO: Add Widgets property to DashboardData
                Configuration = new Dictionary<string, object>
                {
                    ["refresh_interval"] = 30,
                    ["auto_refresh"] = true,
                    ["theme"] = "dark"
                }
            };

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data {DashboardId}", dashboardId);
            throw;
        }
    }

    public async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(string serviceName, TimeSpan timeRange, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Retrieving performance metrics for service: {serviceName}");

            var endTime = DateTime.UtcNow;
            var startTime = endTime.Subtract(timeRange);
            var metrics = new List<PerformanceMetric>();

            var categories = new[] { PerformanceCategory.ResponseTime, PerformanceCategory.Throughput, PerformanceCategory.ErrorRate };
            
            foreach (var category in categories)
            {
                for (var time = startTime; time <= endTime; time = time.AddMinutes(5))
                {
                    metrics.Add(new PerformanceMetric
                    {
                        Id = Guid.NewGuid(),
                        ServiceName = serviceName,
                        MetricType = category.ToString(),
                        Value = GeneratePerformanceValue(category),
                        Timestamp = time,
                        Category = category,
                        Dimensions = new Dictionary<string, string>
                        {
                            ["environment"] = "production",
                            ["region"] = "us-east-1"
                        }
                    });
                }
            }

            return metrics.OrderBy(m => m.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance metrics for service {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<List<ErrorMetric>> GetErrorMetricsAsync(string serviceName, TimeSpan timeRange, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Retrieving error metrics for service: {serviceName}");

            var endTime = DateTime.UtcNow;
            var startTime = endTime.Subtract(timeRange);
            var errors = new List<ErrorMetric>();

            var errorTypes = new[] { "ValidationError", "DatabaseTimeout", "ExternalApiError", "AuthenticationError" };

            foreach (var errorType in errorTypes)
            {
                for (var time = startTime; time <= endTime; time = time.AddHours(1))
                {
                    errors.Add(new ErrorMetric
                    {
                        Id = Guid.NewGuid(),
                        ServiceName = serviceName,
                        ErrorType = errorType,
                        Count = Random.Shared.Next(0, 10),
                        Timestamp = time,
                        ErrorMessage = $"Sample {errorType} occurred",
                        Severity = (ErrorSeverity)Random.Shared.Next(0, 4),
                        Context = new Dictionary<string, object>
                        {
                            ["user_id"] = Guid.NewGuid().ToString(),
                            ["request_id"] = Guid.NewGuid().ToString()
                        }
                    });
                }
            }

            return errors.Where(e => e.Count > 0).OrderBy(e => e.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving error metrics for service {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<SystemOverview> GetSystemOverviewAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, $"Retrieving system overview for tenant: {tenantId}");

            var overview = new SystemOverview
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                GeneratedAt = DateTime.UtcNow,
                Services = GenerateServiceStatuses(),
                Metrics = GenerateSystemMetrics(),
                ActiveAlerts = GenerateActiveAlerts(),
                ResourceUtilization = GenerateResourceUtilization()
            };

            return overview;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system overview for tenant {TenantId}", tenantId);
            throw;
        }
    }

    private List<MetricData> GeneratePerformanceMetrics(string serviceName, DateTime startTime, DateTime endTime)
    {
        var metrics = new List<MetricData>();
        var metricNames = new[] { "response_time", "throughput", "error_rate", "cpu_usage" };

        foreach (var metricName in metricNames)
        {
            for (var time = startTime; time <= endTime; time = time.AddMinutes(1))
            {
                metrics.Add(new MetricData
                {
                    Id = Guid.NewGuid(),
                    ServiceName = serviceName,
                    MetricName = metricName,
                    Value = Random.Shared.NextDouble() * 100,
                    Timestamp = time,
                    Type = MetricType.Gauge,
                    Unit = GetMetricUnit(metricName),
                    Tags = new Dictionary<string, string>
                    {
                        ["environment"] = "production",
                        ["version"] = "1.0.0"
                    }
                });
            }
        }

        return metrics;
    }

    private List<MetricData> GenerateResourceMetrics(string serviceName, DateTime startTime, DateTime endTime)
    {
        var metrics = new List<MetricData>();
        var resourceMetrics = new[] { "memory_usage", "disk_usage", "network_io" };

        foreach (var metricName in resourceMetrics)
        {
            for (var time = startTime; time <= endTime; time = time.AddMinutes(5))
            {
                metrics.Add(new MetricData
                {
                    Id = Guid.NewGuid(),
                    ServiceName = serviceName,
                    MetricName = metricName,
                    Value = Random.Shared.NextDouble() * 100,
                    Timestamp = time,
                    Type = MetricType.Gauge,
                    Unit = GetMetricUnit(metricName),
                    Tags = new Dictionary<string, string>
                    {
                        ["resource_type"] = "system",
                        ["instance"] = $"{serviceName}-001"
                    }
                });
            }
        }

        return metrics;
    }

    private List<MetricData> GenerateBusinessMetrics(string serviceName, DateTime startTime, DateTime endTime)
    {
        var metrics = new List<MetricData>();
        var businessMetrics = new[] { "active_users", "messages_processed", "api_calls" };

        foreach (var metricName in businessMetrics)
        {
            for (var time = startTime; time <= endTime; time = time.AddMinutes(10))
            {
                metrics.Add(new MetricData
                {
                    Id = Guid.NewGuid(),
                    ServiceName = serviceName,
                    MetricName = metricName,
                    Value = Random.Shared.Next(100, 1000),
                    Timestamp = time,
                    Type = MetricType.Counter,
                    Unit = "count",
                    Tags = new Dictionary<string, string>
                    {
                        ["metric_type"] = "business",
                        ["category"] = "usage"
                    }
                });
            }
        }

        return metrics;
    }

    private List<TraceData> GenerateChildTraces(string traceId, string parentSpanId)
    {
        var traces = new List<TraceData>();
        var services = new[] { "auth-service", "chatbot-runtime", "ai-orchestration", "analytics" };

        foreach (var service in services)
        {
            traces.Add(new TraceData
            {
                Id = Guid.NewGuid(),
                TraceId = traceId,
                SpanId = Guid.NewGuid().ToString(),
                ParentSpanId = parentSpanId,
                OperationName = $"{service}.process",
                ServiceName = service,
                StartTime = DateTime.UtcNow.AddMinutes(-4),
                EndTime = DateTime.UtcNow.AddMinutes(-3),
                Duration = TimeSpan.FromSeconds(Random.Shared.Next(1, 30)),
                Status = TraceStatus.Ok,
                Tags = new Dictionary<string, object>
                {
                    ["service.version"] = "1.0.0",
                    ["operation.type"] = "internal"
                }
            });
        }

        return traces;
    }

    private List<LogEntry> GenerateSampleLogs(string serviceName, DateTime startTime, DateTime endTime)
    {
        var logs = new List<LogEntry>();
        var logLevels = new[] { EntityLogLevel.Information, EntityLogLevel.Warning, EntityLogLevel.Error };
        var messages = new[]
        {
            "Request processed successfully",
            "Database connection established",
            "Cache miss occurred",
            "Authentication successful",
            "Rate limit exceeded",
            "External API call failed"
        };

        for (var time = startTime; time <= endTime; time = time.AddMinutes(1))
        {
            logs.Add(new LogEntry
            {
                Id = Guid.NewGuid(),
                ServiceName = serviceName,
                Timestamp = time,
                Level = logLevels[Random.Shared.Next(logLevels.Length)],
                Message = messages[Random.Shared.Next(messages.Length)],
                Category = "Application",
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                Properties = new Dictionary<string, object>
                {
                    ["user_id"] = Guid.NewGuid().ToString(),
                    ["request_id"] = Guid.NewGuid().ToString()
                }
            });
        }

        return logs;
    }

    private List<HealthCheckDependency> GenerateHealthCheckDependencies(string serviceName)
    {
        return new List<HealthCheckDependency>
        {
            new HealthCheckDependency
            {
                Name = "Database",
                Status = HealthStatus.Healthy,
                Description = "SQL Server connection is healthy",
                ResponseTime = TimeSpan.FromMilliseconds(Random.Shared.Next(10, 50))
            },
            new HealthCheckDependency
            {
                Name = "Redis Cache",
                Status = HealthStatus.Healthy,
                Description = "Redis cache is responding",
                ResponseTime = TimeSpan.FromMilliseconds(Random.Shared.Next(5, 20))
            },
            new HealthCheckDependency
            {
                Name = "External API",
                Status = HealthStatus.Degraded,
                Description = "External API response time is elevated",
                ResponseTime = TimeSpan.FromMilliseconds(Random.Shared.Next(200, 500))
            }
        };
    }

    private List<DashboardWidget> GenerateDashboardWidgets()
    {
        return new List<DashboardWidget>
        {
            new DashboardWidget
            {
                Id = Guid.NewGuid(),
                Title = "Average Response Time",
                Type = WidgetType.LineChart,
                Configuration = new WidgetConfiguration(),
                Metrics = new List<DashboardMetric>()
                {
                    new DashboardMetric { Name = "series", Value = 120, Unit = "ms", Timestamp = DateTime.UtcNow },
                    new DashboardMetric { Name = "values", Value = 85, Unit = "ms", Timestamp = DateTime.UtcNow }
                }
            },
            new DashboardWidget
            {
                Id = Guid.NewGuid(),
                Title = "Error Rate",
                Type = WidgetType.Gauge,
                Configuration = new WidgetConfiguration(),
                Metrics = new List<DashboardMetric>()
                {
                    new DashboardMetric { Name = "value", Value = 2.5, Unit = "percent", Timestamp = DateTime.UtcNow },
                    new DashboardMetric { Name = "max", Value = 10, Unit = "percent", Timestamp = DateTime.UtcNow }
                }
            },
            new DashboardWidget
            {
                Id = Guid.NewGuid(),
                Title = "Active Users",
                Type = WidgetType.Counter,
                Configuration = new WidgetConfiguration(),
                Metrics = new List<DashboardMetric>()
                {
                    new DashboardMetric { Name = "value", Value = 1247, Unit = "count", Timestamp = DateTime.UtcNow },
                    new DashboardMetric { Name = "change", Value = 12, Unit = "percent", Timestamp = DateTime.UtcNow }
                }
            }
        };
    }

    private double GeneratePerformanceValue(PerformanceCategory category)
    {
        return category switch
        {
            PerformanceCategory.ResponseTime => Random.Shared.NextDouble() * 1000,
            PerformanceCategory.Throughput => Random.Shared.NextDouble() * 10000,
            PerformanceCategory.ErrorRate => Random.Shared.NextDouble() * 5,
            PerformanceCategory.ResourceUtilization => Random.Shared.NextDouble() * 100,
            _ => Random.Shared.NextDouble() * 100
        };
    }

    private List<ServiceStatus> GenerateServiceStatuses()
    {
        var services = new[] { "api-gateway", "auth-service", "chatbot-runtime", "ai-orchestration", "analytics" };
        
        return services.Select(service => new ServiceStatus
        {
            ServiceName = service,
            Status = HealthStatus.Healthy,
            LastCheck = DateTime.UtcNow,
            Uptime = TimeSpan.FromHours(Random.Shared.Next(1, 720)),
            CpuUsage = Random.Shared.NextDouble() * 100,
            MemoryUsage = Random.Shared.NextDouble() * 100,
            ActiveConnections = Random.Shared.Next(10, 100)
        }).ToList();
    }

    private SystemMetrics GenerateSystemMetrics()
    {
        return new SystemMetrics
        {
            TotalRequests = Random.Shared.Next(10000, 100000),
            RequestsPerSecond = Random.Shared.NextDouble() * 1000,
            AverageResponseTime = Random.Shared.NextDouble() * 500,
            ErrorRate = Random.Shared.NextDouble() * 5,
            ThroughputMbps = Random.Shared.NextDouble() * 100,
            ActiveUsers = Random.Shared.Next(100, 5000)
        };
    }

    private List<ActiveAlert> GenerateActiveAlerts()
    {
        return new List<ActiveAlert>
        {
            new ActiveAlert
            {
                AlertId = Guid.NewGuid().ToString(),
                RuleName = "High Response Time",
                Severity = AlertSeverity.High,
                TriggeredAt = DateTime.UtcNow.AddMinutes(-30),
                Description = "Average response time exceeded 2 seconds",
                ServiceName = "api-gateway"
            }
        };
    }

    private ResourceUtilization GenerateResourceUtilization()
    {
        return new ResourceUtilization
        {
            CpuUtilization = Random.Shared.NextDouble() * 100,
            MemoryUtilization = Random.Shared.NextDouble() * 100,
            DiskUtilization = Random.Shared.NextDouble() * 100,
            NetworkUtilization = Random.Shared.NextDouble() * 100,
            DatabaseConnections = Random.Shared.Next(10, 100),
            CacheHitRate = Random.Shared.Next(80, 99)
        };
    }

    private string GetMetricUnit(string metricName)
    {
        return metricName switch
        {
            "response_time" => "ms",
            "throughput" => "req/s",
            "error_rate" => "%",
            "cpu_usage" => "%",
            "memory_usage" => "%",
            "disk_usage" => "%",
            "network_io" => "MB/s",
            _ => "count"
        };
    }
}
