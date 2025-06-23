using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using Arif.Platform.Shared.Infrastructure.Data;
using Arif.Platform.Shared.Domain.Entities;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Arif.Platform.Shared.Common.Security;

public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly ArifPlatformDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogger(
        ILogger<AuditLogger> logger,
        ArifPlatformDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            EnrichAuditEvent(auditEvent);

            var auditLog = new AuditLog
            {
                Id = auditEvent.Id,
                Timestamp = auditEvent.Timestamp,
                EventType = auditEvent.EventType,
                Action = auditEvent.Action,
                UserId = auditEvent.UserId,
                TenantId = auditEvent.TenantId,
                UserEmail = auditEvent.UserEmail,
                IpAddress = auditEvent.IpAddress,
                UserAgent = auditEvent.UserAgent,
                Resource = auditEvent.Resource,
                Description = auditEvent.Description,
                Details = auditEvent.Details != null ? JsonSerializer.Serialize(auditEvent.Details) : null,
                Severity = auditEvent.Severity.ToString(),
                IsSuccess = auditEvent.IsSuccess,
                ErrorMessage = auditEvent.ErrorMessage
            };

            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync(cancellationToken);

            LogToStructuredLogger(auditEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit event: {EventType} - {Action}", auditEvent.EventType, auditEvent.Action);
        }
    }

    public async Task LogUserActionAsync(string action, Guid userId, Guid tenantId, object? details = null, CancellationToken cancellationToken = default)
    {
        var auditEvent = new AuditEvent
        {
            EventType = "UserAction",
            Action = action,
            UserId = userId,
            TenantId = tenantId,
            Description = $"User performed action: {action}",
            Details = details,
            Severity = AuditSeverity.Information
        };

        await LogAsync(auditEvent, cancellationToken);
    }

    public async Task LogSecurityEventAsync(SecurityEventType eventType, string description, Guid? userId = null, Guid? tenantId = null, object? details = null, CancellationToken cancellationToken = default)
    {
        var severity = GetSecurityEventSeverity(eventType);
        
        var auditEvent = new AuditEvent
        {
            EventType = "SecurityEvent",
            Action = eventType.ToString(),
            UserId = userId,
            TenantId = tenantId,
            Description = description,
            Details = details,
            Severity = severity,
            IsSuccess = !IsSecurityFailureEvent(eventType)
        };

        await LogAsync(auditEvent, cancellationToken);
    }

    public async Task LogDataAccessAsync(string resource, DataAccessType accessType, Guid userId, Guid tenantId, object? details = null, CancellationToken cancellationToken = default)
    {
        var auditEvent = new AuditEvent
        {
            EventType = "DataAccess",
            Action = accessType.ToString(),
            UserId = userId,
            TenantId = tenantId,
            Resource = resource,
            Description = $"Data {accessType.ToString().ToLower()} operation on {resource}",
            Details = details,
            Severity = GetDataAccessSeverity(accessType)
        };

        await LogAsync(auditEvent, cancellationToken);
    }

    private void EnrichAuditEvent(AuditEvent auditEvent)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            auditEvent.IpAddress ??= GetClientIpAddress(httpContext);
            auditEvent.UserAgent ??= httpContext.Request.Headers["User-Agent"].FirstOrDefault();

            if (auditEvent.UserId == null && httpContext.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    auditEvent.UserId = userId;
                }

                auditEvent.UserEmail ??= httpContext.User.FindFirst(ClaimTypes.Email)?.Value;

                var tenantIdClaim = httpContext.User.FindFirst("tenant_id");
                if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
                {
                    auditEvent.TenantId = tenantId;
                }
            }
        }
    }

    private string GetClientIpAddress(HttpContext httpContext)
    {
        var xForwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private void LogToStructuredLogger(AuditEvent auditEvent)
    {
        var logLevel = auditEvent.Severity switch
        {
            AuditSeverity.Critical => MSLogLevel.Critical,
            AuditSeverity.Error => MSLogLevel.Error,
            AuditSeverity.Warning => MSLogLevel.Warning,
            _ => MSLogLevel.Information
        };

        _logger.Log(logLevel, 
            "Audit Event: {EventType} - {Action} | User: {UserId} | Tenant: {TenantId} | IP: {IpAddress} | Success: {IsSuccess} | Description: {Description}",
            auditEvent.EventType,
            auditEvent.Action,
            auditEvent.UserId,
            auditEvent.TenantId,
            auditEvent.IpAddress,
            auditEvent.IsSuccess,
            auditEvent.Description);
    }

    private static AuditSeverity GetSecurityEventSeverity(SecurityEventType eventType)
    {
        return eventType switch
        {
            SecurityEventType.DataBreach => AuditSeverity.Critical,
            SecurityEventType.UnauthorizedAccess => AuditSeverity.Error,
            SecurityEventType.SuspiciousActivity => AuditSeverity.Warning,
            SecurityEventType.LoginFailure => AuditSeverity.Warning,
            SecurityEventType.AccountLockout => AuditSeverity.Warning,
            _ => AuditSeverity.Information
        };
    }

    private static AuditSeverity GetDataAccessSeverity(DataAccessType accessType)
    {
        return accessType switch
        {
            DataAccessType.Delete => AuditSeverity.Warning,
            DataAccessType.Export => AuditSeverity.Warning,
            DataAccessType.Share => AuditSeverity.Warning,
            _ => AuditSeverity.Information
        };
    }

    private static bool IsSecurityFailureEvent(SecurityEventType eventType)
    {
        return eventType switch
        {
            SecurityEventType.LoginFailure => true,
            SecurityEventType.UnauthorizedAccess => true,
            SecurityEventType.SuspiciousActivity => true,
            SecurityEventType.DataBreach => true,
            _ => false
        };
    }
}
