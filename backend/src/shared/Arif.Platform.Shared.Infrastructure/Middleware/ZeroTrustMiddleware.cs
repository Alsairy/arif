using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Arif.Platform.Shared.Domain.Interfaces;
using Arif.Platform.Shared.Infrastructure.Services;
using System.Text.Json;

namespace Arif.Platform.Shared.Infrastructure.Middleware;

public class ZeroTrustMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ZeroTrustMiddleware> _logger;
    private readonly IZeroTrustSecurityService _zeroTrustService;

    public ZeroTrustMiddleware(
        RequestDelegate next,
        ILogger<ZeroTrustMiddleware> logger,
        IZeroTrustSecurityService zeroTrustService)
    {
        _next = next;
        _logger = logger;
        _zeroTrustService = zeroTrustService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (ShouldSkipEvaluation(context))
            {
                await _next(context);
                return;
            }

            var userId = GetUserIdFromContext(context);
            var deviceId = GetDeviceIdFromContext(context);
            var ipAddress = GetClientIpAddress(context);
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            if (userId == null)
            {
                _logger.LogWarning("Zero-trust evaluation skipped: No user ID found in context");
                await _next(context);
                return;
            }

            var trustRequest = new TrustEvaluationRequest
            {
                UserId = userId.Value,
                DeviceId = deviceId ?? "unknown",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Location = await GetLocationFromIpAsync(ipAddress),
                RequestTime = DateTime.UtcNow,
                RequestedResource = context.Request.Path,
                ContextData = ExtractContextData(context)
            };

            var trustScore = await _zeroTrustService.EvaluateTrustScoreAsync(trustRequest);

            var accessRequest = new AccessRequest
            {
                UserId = userId.Value,
                Resource = context.Request.Path,
                Action = context.Request.Method,
                DeviceId = deviceId ?? "unknown",
                IpAddress = ipAddress,
                CurrentTrustScore = trustScore,
                Context = ExtractContextData(context)
            };

            var accessDecision = await _zeroTrustService.MakeAccessDecisionAsync(accessRequest);

            switch (accessDecision.DecisionType)
            {
                case AccessDecisionType.Allow:
                    context.Items["TrustScore"] = trustScore;
                    context.Items["AccessDecision"] = accessDecision;
                    await _next(context);
                    break;

                case AccessDecisionType.Monitor:
                    var sessionId = Guid.NewGuid().ToString();
                    await _zeroTrustService.StartContinuousMonitoringAsync(userId.Value, sessionId);
                    context.Items["MonitoringSessionId"] = sessionId;
                    context.Items["TrustScore"] = trustScore;
                    await _next(context);
                    break;

                case AccessDecisionType.Challenge:
                    await HandleChallengeResponse(context, accessDecision);
                    break;

                case AccessDecisionType.StepUp:
                    await HandleStepUpResponse(context, accessDecision);
                    break;

                case AccessDecisionType.Deny:
                    await HandleDenyResponse(context, accessDecision);
                    break;

                default:
                    await HandleDenyResponse(context, accessDecision);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in zero-trust middleware");
            await _next(context);
        }
    }

    private bool ShouldSkipEvaluation(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        var skipPaths = new[]
        {
            "/health",
            "/metrics",
            "/swagger",
            "/api/auth/login",
            "/api/auth/register",
            "/api/public"
        };

        return skipPaths.Any(skipPath => path.StartsWith(skipPath));
    }

    private Guid? GetUserIdFromContext(HttpContext context)
    {
        var userIdClaim = context.User?.FindFirst("sub")?.Value ?? 
                         context.User?.FindFirst("userId")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        var userIdHeader = context.Request.Headers["X-User-Id"].FirstOrDefault();
        if (Guid.TryParse(userIdHeader, out userId))
        {
            return userId;
        }

        return null;
    }

    private string? GetDeviceIdFromContext(HttpContext context)
    {
        return context.Request.Headers["X-Device-Id"].FirstOrDefault() ??
               context.Request.Cookies["DeviceId"];
    }

    private string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private async Task<string> GetLocationFromIpAsync(string ipAddress)
    {
        return await Task.FromResult("Unknown");
    }

    private Dictionary<string, object> ExtractContextData(HttpContext context)
    {
        return new Dictionary<string, object>
        {
            ["Method"] = context.Request.Method,
            ["Path"] = context.Request.Path.Value ?? "",
            ["QueryString"] = context.Request.QueryString.Value ?? "",
            ["UserAgent"] = context.Request.Headers["User-Agent"].ToString(),
            ["Referer"] = context.Request.Headers["Referer"].ToString(),
            ["AcceptLanguage"] = context.Request.Headers["Accept-Language"].ToString(),
            ["ContentType"] = context.Request.ContentType ?? "",
            ["ContentLength"] = context.Request.ContentLength ?? 0,
            ["IsHttps"] = context.Request.IsHttps,
            ["Protocol"] = context.Request.Protocol
        };
    }

    private async Task HandleChallengeResponse(HttpContext context, AccessDecision decision)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "additional_authentication_required",
            message = decision.Reason,
            required_actions = decision.RequiredActions,
            challenge_type = "mfa_required"
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleStepUpResponse(HttpContext context, AccessDecision decision)
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "step_up_authentication_required",
            message = decision.Reason,
            required_actions = decision.RequiredActions,
            step_up_url = "/api/auth/step-up"
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleDenyResponse(HttpContext context, AccessDecision decision)
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "access_denied",
            message = decision.Reason,
            trust_level = "insufficient"
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

public static class ZeroTrustMiddlewareExtensions
{
    public static IApplicationBuilder UseZeroTrust(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ZeroTrustMiddleware>();
    }
}
