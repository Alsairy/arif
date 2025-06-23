using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text;
using System.Diagnostics;

namespace Arif.Platform.Shared.Infrastructure.Configuration;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddComprehensiveHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            healthChecksBuilder.AddSqlServer(
                connectionString,
                name: "sqlserver",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "sql" });
        }

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            healthChecksBuilder.AddRedis(
                redisConnectionString,
                name: "redis",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "cache", "redis" });
        }

        var externalServices = configuration.GetSection("ExternalServices").GetChildren();
        foreach (var service in externalServices)
        {
            var url = service["HealthCheckUrl"];
            if (!string.IsNullOrEmpty(url))
            {
                healthChecksBuilder.AddCheck(service.Key.ToLowerInvariant(), () =>
                {
                    try
                    {
                        using var client = new HttpClient();
                        var response = client.GetAsync(url).Result;
                        return response.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded();
                    }
                    catch
                    {
                        return HealthCheckResult.Unhealthy();
                    }
                }, tags: new[] { "external", service.Key.ToLowerInvariant() });
            }
        }

        healthChecksBuilder.AddCheck<DatabaseHealthCheck>("database-detailed");
        healthChecksBuilder.AddCheck<MemoryHealthCheck>("memory");
        healthChecksBuilder.AddCheck<DiskSpaceHealthCheck>("disk-space");
        healthChecksBuilder.AddCheck<ApplicationHealthCheck>("application");

        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(30);
            options.MaximumHistoryEntriesPerEndpoint(50);
            options.AddHealthCheckEndpoint("Arif Platform API", "/health");
            options.AddHealthCheckEndpoint("Arif Platform Detailed", "/health/detailed");
        }).AddInMemoryStorage();

        return services;
    }

    public static IApplicationBuilder UseComprehensiveHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteHealthCheckResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthCheckResponse
        });

        app.UseHealthChecks("/health/detailed", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteDetailedHealthCheckResponse
        });

        app.UseHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-ui-api";
        });

        return app;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                tags = entry.Value.Tags
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }

    private static async Task WriteDetailedHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            machineName = Environment.MachineName,
            processId = Environment.ProcessId,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                data = entry.Value.Data,
                tags = entry.Value.Tags,
                exception = entry.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
}

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public DatabaseHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                return HealthCheckResult.Unhealthy("Database connection string not configured");
            }

            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("Database connection successful", new Dictionary<string, object>
            {
                ["server"] = connection.DataSource,
                ["database"] = connection.Database,
                ["connectionTimeout"] = connection.ConnectionTimeout
            });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}

public class MemoryHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var allocatedBytes = GC.GetTotalMemory(false);
        var workingSet = Environment.WorkingSet;
        
        var data = new Dictionary<string, object>
        {
            ["allocatedBytes"] = allocatedBytes,
            ["workingSetBytes"] = workingSet,
            ["gen0Collections"] = GC.CollectionCount(0),
            ["gen1Collections"] = GC.CollectionCount(1),
            ["gen2Collections"] = GC.CollectionCount(2)
        };

        var status = allocatedBytes < 1024 * 1024 * 1024 ? HealthStatus.Healthy : HealthStatus.Degraded;
        
        return Task.FromResult(new HealthCheckResult(status, "Memory usage check", data: data));
    }
}

public class DiskSpaceHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
            var data = new Dictionary<string, object>();
            var overallStatus = HealthStatus.Healthy;

            foreach (var drive in drives)
            {
                var freeSpacePercentage = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;
                data[$"{drive.Name}_freeSpaceBytes"] = drive.AvailableFreeSpace;
                data[$"{drive.Name}_totalSpaceBytes"] = drive.TotalSize;
                data[$"{drive.Name}_freeSpacePercentage"] = Math.Round(freeSpacePercentage, 2);

                if (freeSpacePercentage < 10)
                {
                    overallStatus = HealthStatus.Unhealthy;
                }
                else if (freeSpacePercentage < 20)
                {
                    overallStatus = HealthStatus.Degraded;
                }
            }

            return Task.FromResult(new HealthCheckResult(overallStatus, "Disk space check", data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Disk space check failed", ex));
        }
    }
}

public class ApplicationHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["uptime"] = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime(),
            ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            ["machineName"] = Environment.MachineName,
            ["processId"] = Environment.ProcessId,
            ["threadCount"] = Process.GetCurrentProcess().Threads.Count,
            ["handleCount"] = Process.GetCurrentProcess().HandleCount
        };

        return Task.FromResult(HealthCheckResult.Healthy("Application is running", data: data));
    }
}
