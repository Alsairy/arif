using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Arif.Platform.Shared.Common.Security;
using System.Net;

namespace Arif.Platform.Shared.Infrastructure.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IRateLimitingService rateLimitingService,
        ILogger<RateLimitingMiddleware> logger,
        IOptions<RateLimitOptions> options)
    {
        _next = next;
        _rateLimitingService = rateLimitingService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = GetRateLimitKey(context);
        var rateLimitInfo = await _rateLimitingService.GetRateLimitInfoAsync(
            key, _options.MaxRequests, _options.TimeWindow);

        context.Response.Headers.Append("X-RateLimit-Limit", _options.MaxRequests.ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", rateLimitInfo.RequestsRemaining.ToString());
        context.Response.Headers.Append("X-RateLimit-Reset", ((DateTimeOffset)rateLimitInfo.ResetTime).ToUnixTimeSeconds().ToString());

        if (!rateLimitInfo.IsAllowed)
        {
            _logger.LogWarning("Rate limit exceeded for key: {Key}", key);
            
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Append("Retry-After", ((int)rateLimitInfo.RetryAfter.TotalSeconds).ToString());
            
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await _next(context);
    }

    private string GetRateLimitKey(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        var userId = context.User?.Identity?.Name ?? "anonymous";
        return $"{clientIp}:{userId}";
    }

    private string GetClientIpAddress(HttpContext context)
    {
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
