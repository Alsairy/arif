using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using Arif.Platform.Shared.Common.Security;

namespace Arif.Platform.Shared.Infrastructure.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse();

        switch (exception)
        {
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Unauthorized access";
                await LogSecurityEvent(context, "Unauthorized access attempt", exception);
                break;
            
            case ArgumentException:
            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Invalid request";
                break;
            
            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "Resource not found";
                break;
            
            case TimeoutException:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                response.Message = "Request timeout";
                break;
            
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An internal server error occurred";
                await LogSecurityEvent(context, "Internal server error", exception);
                break;
        }

        context.Response.StatusCode = response.StatusCode;
        
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await context.Response.WriteAsync(jsonResponse);
    }

    private async Task LogSecurityEvent(HttpContext context, string description, Exception exception)
    {
        try
        {
            var auditLogger = context.RequestServices.GetService<IAuditLogger>();
            if (auditLogger == null)
            {
                _logger.LogWarning("IAuditLogger service not available for security event logging");
                return;
            }

            var userId = context.User?.Identity?.Name;
            var tenantId = context.User?.FindFirst("tenant_id")?.Value;

            if (Guid.TryParse(userId, out var userGuid) && Guid.TryParse(tenantId, out var tenantGuid))
            {
                await auditLogger.LogSecurityEventAsync(
                    SecurityEventType.SuspiciousActivity,
                    description,
                    userGuid,
                    tenantGuid,
                    new { 
                        ExceptionType = exception.GetType().Name,
                        ExceptionMessage = exception.Message,
                        RequestPath = context.Request.Path,
                        RequestMethod = context.Request.Method
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security event for exception handling");
        }
    }

    private class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
