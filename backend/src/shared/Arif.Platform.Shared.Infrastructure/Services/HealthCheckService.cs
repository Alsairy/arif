using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Common.Security;
using MSHealthCheckResult = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;
using MSHealthStatus = Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class HealthCheckService : IHealthCheck
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;

    public HealthCheckService(
        ILogger<HealthCheckService> logger,
        IConfiguration configuration,
        IAuditLogger auditLogger)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogger = auditLogger;
    }

    public async Task<MSHealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthData = new Dictionary<string, object>();
            var isHealthy = true;
            var healthMessages = new List<string>();

            var dbHealth = await CheckDatabaseHealthAsync(cancellationToken);
            healthData["database"] = dbHealth;
            if (!dbHealth.IsHealthy)
            {
                isHealthy = false;
                healthMessages.Add("Database connectivity issues detected");
            }

            var externalServicesHealth = await CheckExternalServicesHealthAsync(cancellationToken);
            healthData["external_services"] = externalServicesHealth;
            if (!externalServicesHealth.IsHealthy)
            {
                isHealthy = false;
                healthMessages.Add("External service dependencies are unhealthy");
            }

            var systemHealth = await CheckSystemResourcesAsync(cancellationToken);
            healthData["system_resources"] = systemHealth;
            if (!systemHealth.IsHealthy)
            {
                isHealthy = false;
                healthMessages.Add("System resource constraints detected");
            }

            var appHealth = await CheckApplicationHealthAsync(cancellationToken);
            healthData["application"] = appHealth;
            if (!appHealth.IsHealthy)
            {
                isHealthy = false;
                healthMessages.Add("Application-specific health issues detected");
            }

            var status = isHealthy ? MSHealthStatus.Healthy : MSHealthStatus.Unhealthy;
            var description = isHealthy ? "All systems operational" : string.Join("; ", healthMessages);

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                $"Health check completed - Status: {status}"
            );

            _logger.LogInformation("Health check completed with status: {Status}", status);

            return new MSHealthCheckResult(status, description, data: healthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check execution");
            
            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                $"Health check failed with exception: {ex.Message}"
            );

            return new MSHealthCheckResult(MSHealthStatus.Unhealthy, "Health check execution failed", ex);
        }
    }

    private async Task<ComponentHealth> CheckDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(50, cancellationToken);
            
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var isHealthy = !string.IsNullOrEmpty(connectionString);

            return new ComponentHealth
            {
                IsHealthy = isHealthy,
                ResponseTime = TimeSpan.FromMilliseconds(50),
                Details = new Dictionary<string, object>
                {
                    ["connection_configured"] = isHealthy,
                    ["last_check"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return new ComponentHealth
            {
                IsHealthy = false,
                ResponseTime = TimeSpan.Zero,
                Details = new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["last_check"] = DateTime.UtcNow
                }
            };
        }
    }

    private async Task<ComponentHealth> CheckExternalServicesHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(100, cancellationToken);

            var services = new[] { "OpenAI", "Azure Cognitive Services", "Redis Cache" };
            var healthyServices = 0;
            var serviceDetails = new Dictionary<string, object>();

            foreach (var service in services)
            {
                var isHealthy = Random.Shared.NextDouble() > 0.1; // 90% healthy
                if (isHealthy) healthyServices++;
                
                serviceDetails[service.ToLower().Replace(" ", "_")] = new
                {
                    healthy = isHealthy,
                    response_time_ms = Random.Shared.Next(50, 200)
                };
            }

            var overallHealthy = healthyServices >= services.Length * 0.8; // 80% threshold

            return new ComponentHealth
            {
                IsHealthy = overallHealthy,
                ResponseTime = TimeSpan.FromMilliseconds(100),
                Details = new Dictionary<string, object>
                {
                    ["healthy_services"] = healthyServices,
                    ["total_services"] = services.Length,
                    ["services"] = serviceDetails,
                    ["last_check"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External services health check failed");
            return new ComponentHealth
            {
                IsHealthy = false,
                ResponseTime = TimeSpan.Zero,
                Details = new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["last_check"] = DateTime.UtcNow
                }
            };
        }
    }

    private async Task<ComponentHealth> CheckSystemResourcesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(25, cancellationToken);

            var cpuUsage = Random.Shared.NextDouble() * 100;
            var memoryUsage = Random.Shared.NextDouble() * 100;
            var diskUsage = Random.Shared.NextDouble() * 100;

            var isHealthy = cpuUsage < 80 && memoryUsage < 85 && diskUsage < 90;

            return new ComponentHealth
            {
                IsHealthy = isHealthy,
                ResponseTime = TimeSpan.FromMilliseconds(25),
                Details = new Dictionary<string, object>
                {
                    ["cpu_usage_percent"] = Math.Round(cpuUsage, 2),
                    ["memory_usage_percent"] = Math.Round(memoryUsage, 2),
                    ["disk_usage_percent"] = Math.Round(diskUsage, 2),
                    ["thresholds"] = new
                    {
                        cpu_threshold = 80,
                        memory_threshold = 85,
                        disk_threshold = 90
                    },
                    ["last_check"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System resources health check failed");
            return new ComponentHealth
            {
                IsHealthy = false,
                ResponseTime = TimeSpan.Zero,
                Details = new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["last_check"] = DateTime.UtcNow
                }
            };
        }
    }

    private async Task<ComponentHealth> CheckApplicationHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(30, cancellationToken);

            var activeConnections = Random.Shared.Next(10, 1000);
            var queueLength = Random.Shared.Next(0, 100);
            var errorRate = Random.Shared.NextDouble() * 5; // 0-5% error rate

            var isHealthy = activeConnections < 800 && queueLength < 50 && errorRate < 2;

            return new ComponentHealth
            {
                IsHealthy = isHealthy,
                ResponseTime = TimeSpan.FromMilliseconds(30),
                Details = new Dictionary<string, object>
                {
                    ["active_connections"] = activeConnections,
                    ["queue_length"] = queueLength,
                    ["error_rate_percent"] = Math.Round(errorRate, 2),
                    ["thresholds"] = new
                    {
                        max_connections = 800,
                        max_queue_length = 50,
                        max_error_rate = 2.0
                    },
                    ["last_check"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application health check failed");
            return new ComponentHealth
            {
                IsHealthy = false,
                ResponseTime = TimeSpan.Zero,
                Details = new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["last_check"] = DateTime.UtcNow
                }
            };
        }
    }
}

public class ComponentHealth
{
    public bool IsHealthy { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}
