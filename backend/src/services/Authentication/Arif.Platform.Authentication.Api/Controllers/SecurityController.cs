using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arif.Platform.Shared.Common.Security;
using System.Security.Claims;

namespace Arif.Platform.Authentication.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecurityController : ControllerBase
{
    private readonly ISecurityMonitoringService _securityMonitoringService;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<SecurityController> _logger;

    public SecurityController(
        ISecurityMonitoringService securityMonitoringService,
        IAuditLogger auditLogger,
        ILogger<SecurityController> logger)
    {
        _securityMonitoringService = securityMonitoringService;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    [HttpPost("report-suspicious-activity")]
    public async Task<IActionResult> ReportSuspiciousActivity([FromBody] SuspiciousActivityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var tenantId = GetCurrentTenantId();

            await _securityMonitoringService.MonitorSuspiciousActivityAsync(
                request.Activity, 
                userId, 
                tenantId, 
                request.Details, 
                cancellationToken);

            return Ok(new { message = "Suspicious activity reported successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to report suspicious activity");
            return StatusCode(500, new { message = "Failed to report suspicious activity" });
        }
    }

    [HttpPost("check-anomalous-activity")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CheckAnomalousActivity([FromQuery] Guid? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetUserId = userId ?? GetCurrentUserId();
            if (targetUserId == null)
                return BadRequest(new { message = "User ID is required" });

            await _securityMonitoringService.CheckForAnomalousActivityAsync(targetUserId.Value, cancellationToken);
            return Ok(new { message = "Anomalous activity check completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check anomalous activity");
            return StatusCode(500, new { message = "Failed to check anomalous activity" });
        }
    }

    [HttpPost("generate-security-report")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateSecurityReport([FromBody] SecurityReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _securityMonitoringService.GenerateSecurityReportAsync(
                request.FromDate, 
                request.ToDate, 
                cancellationToken);

            return Ok(new { message = "Security report generated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate security report");
            return StatusCode(500, new { message = "Failed to generate security report" });
        }
    }

    [HttpGet("audit-logs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            await _auditLogger.LogUserActionAsync("ViewAuditLogs", GetCurrentUserId() ?? Guid.Empty, GetCurrentTenantId() ?? Guid.Empty, 
                new { FromDate = fromDate, ToDate = toDate, Page = page, PageSize = pageSize });

            return Ok(new { message = "Audit logs retrieved successfully", page, pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit logs");
            return StatusCode(500, new { message = "Failed to get audit logs" });
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private Guid? GetCurrentTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
    }

    public class SuspiciousActivityRequest
    {
        public string Activity { get; set; } = string.Empty;
        public object? Details { get; set; }
    }

    public class SecurityReportRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
