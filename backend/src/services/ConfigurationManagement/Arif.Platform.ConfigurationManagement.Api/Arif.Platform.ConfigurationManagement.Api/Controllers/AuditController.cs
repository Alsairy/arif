using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Domain.DTOs;

namespace Arif.Platform.ConfigurationManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IConfigurationAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IConfigurationAuditService auditService,
        ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ConfigurationAuditLogDto>>> GetAuditLogs([FromQuery] AuditLogFilter filter)
    {
        try
        {
            var auditLogs = await _auditService.GetAuditLogsAsync(filter);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("configurations/{configurationId}")]
    public async Task<ActionResult<List<ConfigurationAuditLogDto>>> GetConfigurationAuditLogs(Guid configurationId)
    {
        try
        {
            var auditLogs = await _auditService.GetConfigurationAuditLogsAsync(configurationId);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration audit logs {ConfigurationId}", configurationId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("feature-flags/{featureFlagId}")]
    public async Task<ActionResult<List<ConfigurationAuditLogDto>>> GetFeatureFlagAuditLogs(Guid featureFlagId)
    {
        try
        {
            var auditLogs = await _auditService.GetFeatureFlagAuditLogsAsync(featureFlagId);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature flag audit logs {FeatureFlagId}", featureFlagId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("deployments/{deploymentId}")]
    public async Task<ActionResult<List<ConfigurationAuditLogDto>>> GetDeploymentAuditLogs(Guid deploymentId)
    {
        try
        {
            var auditLogs = await _auditService.GetDeploymentAuditLogsAsync(deploymentId);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment audit logs {DeploymentId}", deploymentId);
            return StatusCode(500, "Internal server error");
        }
    }
}
