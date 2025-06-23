using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Arif.Platform.Shared.Infrastructure.Data;
using Arif.Platform.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arif.Platform.Shared.Common.Security;

public class SecurityMonitoringService : ISecurityMonitoringService
{
    private readonly ILogger<SecurityMonitoringService> _logger;
    private readonly IAuditLogger _auditLogger;
    private readonly IDistributedCache _cache;
    private readonly ArifPlatformDbContext _dbContext;

    private const int MAX_FAILED_LOGINS = 5;
    private const int FAILED_LOGIN_WINDOW_MINUTES = 15;
    private const int SUSPICIOUS_ACTIVITY_THRESHOLD = 10;

    public SecurityMonitoringService(
        ILogger<SecurityMonitoringService> logger,
        IAuditLogger auditLogger,
        IDistributedCache cache,
        ArifPlatformDbContext dbContext)
    {
        _logger = logger;
        _auditLogger = auditLogger;
        _cache = cache;
        _dbContext = dbContext;
    }

    public async Task MonitorSuspiciousActivityAsync(string activity, Guid? userId = null, Guid? tenantId = null, object? details = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.SuspiciousActivity,
                activity,
                userId,
                tenantId,
                details,
                cancellationToken);

            var alert = new SecurityAlert
            {
                AlertType = SecurityAlertType.SuspiciousLogin,
                Severity = SecurityAlertSeverity.Medium,
                Description = activity,
                UserId = userId,
                TenantId = tenantId,
                Details = details
            };

            await CreateSecurityAlertAsync(alert, cancellationToken);
            _logger.LogWarning("Suspicious activity detected: {Activity} for User: {UserId}", activity, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to monitor suspicious activity");
        }
    }

    public async Task MonitorFailedLoginAttemptsAsync(string email, string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"failed_logins:{email}:{ipAddress}";
            var failedAttemptsJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            
            var failedAttempts = 1;
            if (!string.IsNullOrEmpty(failedAttemptsJson))
            {
                var cachedData = JsonSerializer.Deserialize<FailedLoginData>(failedAttemptsJson);
                if (cachedData != null && cachedData.LastAttempt > DateTime.UtcNow.AddMinutes(-FAILED_LOGIN_WINDOW_MINUTES))
                {
                    failedAttempts = cachedData.Count + 1;
                }
            }

            var newData = new FailedLoginData
            {
                Count = failedAttempts,
                LastAttempt = DateTime.UtcNow
            };

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(FAILED_LOGIN_WINDOW_MINUTES)
            };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(newData), cacheOptions, cancellationToken);

            if (failedAttempts >= MAX_FAILED_LOGINS)
            {
                var alert = new SecurityAlert
                {
                    AlertType = SecurityAlertType.MultipleFailedLogins,
                    Severity = SecurityAlertSeverity.High,
                    Description = $"Multiple failed login attempts detected for {email} from {ipAddress}",
                    IpAddress = ipAddress,
                    Details = new { Email = email, FailedAttempts = failedAttempts }
                };

                await CreateSecurityAlertAsync(alert, cancellationToken);
                _logger.LogWarning("Multiple failed login attempts detected for {Email} from {IpAddress}: {Count} attempts", 
                    email, ipAddress, failedAttempts);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to monitor failed login attempts");
        }
    }

    public async Task MonitorUnauthorizedAccessAsync(string resource, Guid? userId = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.UnauthorizedAccess,
                $"Unauthorized access attempt to {resource}",
                userId,
                null,
                new { Resource = resource, IpAddress = ipAddress },
                cancellationToken);

            var alert = new SecurityAlert
            {
                AlertType = SecurityAlertType.UnauthorizedAccess,
                Severity = SecurityAlertSeverity.High,
                Description = $"Unauthorized access attempt to {resource}",
                UserId = userId,
                IpAddress = ipAddress,
                Details = new { Resource = resource }
            };

            await CreateSecurityAlertAsync(alert, cancellationToken);
            _logger.LogWarning("Unauthorized access attempt to {Resource} by User: {UserId} from IP: {IpAddress}", 
                resource, userId, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to monitor unauthorized access");
        }
    }

    public async Task MonitorDataAccessPatternsAsync(Guid userId, string resource, DataAccessType accessType, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"data_access:{userId}:{DateTime.UtcNow:yyyy-MM-dd-HH}";
            var accessCountJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            
            var accessCount = 1;
            if (!string.IsNullOrEmpty(accessCountJson))
            {
                accessCount = JsonSerializer.Deserialize<int>(accessCountJson) + 1;
            }

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(accessCount), cacheOptions, cancellationToken);

            if (accessCount > SUSPICIOUS_ACTIVITY_THRESHOLD)
            {
                await MonitorSuspiciousActivityAsync(
                    $"High volume data access detected: {accessCount} operations in 1 hour",
                    userId,
                    null,
                    new { Resource = resource, AccessType = accessType, Count = accessCount },
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to monitor data access patterns");
        }
    }

    public async Task CheckForAnomalousActivityAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var recentAuditLogs = await _dbContext.AuditLogs
                .Where(al => al.UserId == userId && al.Timestamp > DateTime.UtcNow.AddHours(-24))
                .OrderByDescending(al => al.Timestamp)
                .Take(100)
                .ToListAsync(cancellationToken);

            var suspiciousPatterns = AnalyzeActivityPatterns(recentAuditLogs);
            
            foreach (var pattern in suspiciousPatterns)
            {
                await MonitorSuspiciousActivityAsync(
                    pattern.Description,
                    userId,
                    null,
                    pattern.Details,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for anomalous activity");
        }
    }

    public async Task GenerateSecurityReportAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var securityEvents = await _dbContext.AuditLogs
                .Where(al => al.Timestamp >= fromDate && al.Timestamp <= toDate && al.EventType == "SecurityEvent")
                .GroupBy(al => al.Action)
                .Select(g => new { EventType = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var report = new
            {
                Period = new { From = fromDate, To = toDate },
                SecurityEvents = securityEvents,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Security report generated for period {FromDate} to {ToDate}: {Report}", 
                fromDate, toDate, JsonSerializer.Serialize(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate security report");
        }
    }

    private async Task CreateSecurityAlertAsync(SecurityAlert alert, CancellationToken cancellationToken)
    {
        try
        {
            var alertEntity = new SecurityAlertEntity
            {
                Id = alert.Id,
                Timestamp = alert.Timestamp,
                AlertType = alert.AlertType.ToString(),
                Severity = alert.Severity.ToString(),
                Description = alert.Description,
                UserId = alert.UserId,
                TenantId = alert.TenantId,
                IpAddress = alert.IpAddress,
                Details = alert.Details != null ? JsonSerializer.Serialize(alert.Details) : null,
                IsResolved = alert.IsResolved,
                ResolvedAt = alert.ResolvedAt,
                ResolvedBy = alert.ResolvedBy
            };

            _dbContext.SecurityAlerts.Add(alertEntity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create security alert");
        }
    }

    private List<SuspiciousPattern> AnalyzeActivityPatterns(List<Arif.Platform.Shared.Domain.Entities.AuditLog> auditLogs)
    {
        var patterns = new List<SuspiciousPattern>();

        var rapidActions = auditLogs
            .GroupBy(al => al.Action)
            .Where(g => g.Count() > 20)
            .Select(g => new SuspiciousPattern
            {
                Description = $"Rapid repeated action detected: {g.Key} performed {g.Count()} times",
                Details = new { Action = g.Key, Count = g.Count() }
            });

        patterns.AddRange(rapidActions);

        var offHoursActivity = auditLogs
            .Where(al => al.Timestamp.Hour < 6 || al.Timestamp.Hour > 22)
            .GroupBy(al => al.Timestamp.Date)
            .Where(g => g.Count() > 10)
            .Select(g => new SuspiciousPattern
            {
                Description = $"High off-hours activity detected on {g.Key:yyyy-MM-dd}",
                Details = new { Date = g.Key, Count = g.Count() }
            });

        patterns.AddRange(offHoursActivity);

        return patterns;
    }

    private class FailedLoginData
    {
        public int Count { get; set; }
        public DateTime LastAttempt { get; set; }
    }

    private class SuspiciousPattern
    {
        public string Description { get; set; } = string.Empty;
        public object? Details { get; set; }
    }
}
