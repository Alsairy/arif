using Microsoft.AspNetCore.Http;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.ContainsKey("TenantId") == true)
            {
                return (Guid)context.Items["TenantId"]!;
            }
            return Guid.Empty;
        }
    }

    public string TenantSubdomain
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.ContainsKey("TenantSubdomain") == true)
            {
                return (string)context.Items["TenantSubdomain"]!;
            }
            return string.Empty;
        }
    }

    public bool IsSystemContext => TenantId == Guid.Empty;

    public void SetTenantId(Guid tenantId)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            context.Items["TenantId"] = tenantId;
        }
    }

    public void SetTenantSubdomain(string subdomain)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            context.Items["TenantSubdomain"] = subdomain;
        }
    }
}
