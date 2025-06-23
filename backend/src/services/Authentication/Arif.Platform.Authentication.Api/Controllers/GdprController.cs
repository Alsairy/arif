using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arif.Platform.Shared.Common.Security;
using System.Security.Claims;

namespace Arif.Platform.Authentication.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GdprController : ControllerBase
{
    private readonly IGdprService _gdprService;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<GdprController> _logger;

    public GdprController(
        IGdprService gdprService,
        IAuditLogger auditLogger,
        ILogger<GdprController> logger)
    {
        _gdprService = gdprService;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportUserData(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var export = await _gdprService.ExportUserDataAsync(userId.Value, cancellationToken);
            
            var fileName = $"user_data_export_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(export, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            return File(System.Text.Encoding.UTF8.GetBytes(jsonContent), "application/json", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export user data");
            return StatusCode(500, new { message = "Failed to export user data" });
        }
    }

    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteUserAccount([FromQuery] bool hardDelete = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var success = await _gdprService.DeleteUserDataAsync(userId.Value, hardDelete, cancellationToken);
            
            if (success)
            {
                return Ok(new { message = hardDelete ? "Account permanently deleted" : "Account anonymized" });
            }
            
            return BadRequest(new { message = "Failed to delete account" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user account");
            return StatusCode(500, new { message = "Failed to delete account" });
        }
    }

    [HttpPost("consent")]
    public async Task<IActionResult> RecordConsent([FromBody] ConsentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var consent = await _gdprService.RecordConsentAsync(
                userId.Value, 
                request.ConsentType, 
                request.IsGranted, 
                request.Details, 
                cancellationToken);

            return Ok(consent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record consent");
            return StatusCode(500, new { message = "Failed to record consent" });
        }
    }

    [HttpPost("consent/withdraw")]
    public async Task<IActionResult> WithdrawConsent([FromBody] WithdrawConsentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var success = await _gdprService.WithdrawConsentAsync(userId.Value, request.ConsentType, cancellationToken);
            
            if (success)
            {
                return Ok(new { message = "Consent withdrawn successfully" });
            }
            
            return BadRequest(new { message = "Failed to withdraw consent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to withdraw consent");
            return StatusCode(500, new { message = "Failed to withdraw consent" });
        }
    }

    [HttpGet("consents")]
    public async Task<IActionResult> GetUserConsents(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var consents = await _gdprService.GetUserConsentsAsync(userId.Value, cancellationToken);
            return Ok(consents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user consents");
            return StatusCode(500, new { message = "Failed to get consents" });
        }
    }

    [HttpGet("data-processing-history")]
    public async Task<IActionResult> GetDataProcessingHistory(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var history = await _gdprService.GetDataProcessingHistoryAsync(userId.Value, cancellationToken);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get data processing history");
            return StatusCode(500, new { message = "Failed to get data processing history" });
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public class ConsentRequest
    {
        public ConsentType ConsentType { get; set; }
        public bool IsGranted { get; set; }
        public string? Details { get; set; }
    }

    public class WithdrawConsentRequest
    {
        public ConsentType ConsentType { get; set; }
    }
}
