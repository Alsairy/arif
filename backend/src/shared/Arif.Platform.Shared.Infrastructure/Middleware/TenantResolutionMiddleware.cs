using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Arif.Platform.Shared.Infrastructure.Services;

namespace Arif.Platform.Shared.Infrastructure.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        try
        {
            var tenantId = ResolveTenantId(context);
            if (tenantId.HasValue)
            {
                tenantContext.SetTenantId(tenantId.Value);
                _logger.LogDebug("Tenant resolved: {TenantId}", tenantId.Value);
            }
            else
            {
                _logger.LogDebug("No tenant resolved for request: {Path}", context.Request.Path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant for request: {Path}", context.Request.Path);
        }

        await _next(context);
    }

    private Guid? ResolveTenantId(HttpContext context)
    {
        
        var host = context.Request.Host.Host;
        if (!string.IsNullOrEmpty(host) && host.Contains('.'))
        {
            var subdomain = host.Split('.')[0];
            if (!string.IsNullOrEmpty(subdomain) && 
                subdomain != "www" && 
                subdomain != "api" && 
                subdomain != "admin")
            {
                _logger.LogDebug("Subdomain detected: {Subdomain}", subdomain);
            }
        }

        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader))
        {
            if (Guid.TryParse(tenantHeader.FirstOrDefault(), out var tenantId))
            {
                return tenantId;
            }
        }

        if (context.Request.Query.TryGetValue("tenantId", out var tenantQuery))
        {
            if (Guid.TryParse(tenantQuery.FirstOrDefault(), out var tenantId))
            {
                return tenantId;
            }
        }

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var tenantId))
            {
                return tenantId;
            }
        }

        if (context.Request.RouteValues.TryGetValue("tenantId", out var routeTenantId))
        {
            if (Guid.TryParse(routeTenantId?.ToString(), out var tenantId))
            {
                return tenantId;
            }
        }

        return null;
    }
}
