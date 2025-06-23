using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Domain.DTOs;

namespace Arif.Platform.ConfigurationManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(
        IConfigurationService configurationService,
        ILogger<ConfigurationController> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<ConfigurationDto>> GetConfiguration(
        string key,
        [FromQuery] string environment,
        [FromQuery] string application,
        [FromQuery] Guid? tenantId = null)
    {
        try
        {
            var configuration = await _configurationService.GetConfigurationAsync(key, environment, application, tenantId);
            return Ok(configuration);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Configuration with key '{key}' not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration {Key}", key);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ConfigurationDto>>> GetConfigurations(
        [FromQuery] string environment,
        [FromQuery] string application,
        [FromQuery] Guid? tenantId = null)
    {
        try
        {
            var configurations = await _configurationService.GetConfigurationsAsync(environment, application, tenantId);
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configurations for {Environment}/{Application}", environment, application);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ConfigurationDto>> CreateConfiguration([FromBody] CreateConfigurationDto dto)
    {
        try
        {
            var configuration = await _configurationService.CreateConfigurationAsync(dto);
            return CreatedAtAction(nameof(GetConfiguration), 
                new { key = configuration.Key, environment = configuration.Environment, application = configuration.Application }, 
                configuration);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating configuration");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ConfigurationDto>> UpdateConfiguration(Guid id, [FromBody] UpdateConfigurationDto dto)
    {
        try
        {
            var configuration = await _configurationService.UpdateConfigurationAsync(id, dto);
            return Ok(configuration);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Configuration with ID '{id}' not found");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteConfiguration(Guid id)
    {
        try
        {
            var result = await _configurationService.DeleteConfigurationAsync(id);
            if (!result)
                return NotFound($"Configuration with ID '{id}' not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting configuration {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/validate")]
    public async Task<ActionResult<bool>> ValidateConfiguration(Guid id)
    {
        try
        {
            var isValid = await _configurationService.ValidateConfigurationAsync(id);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating configuration {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("validate")]
    public async Task<ActionResult<List<ConfigurationValidationResult>>> ValidateConfigurations(
        [FromQuery] string environment,
        [FromQuery] string application)
    {
        try
        {
            var results = await _configurationService.ValidateConfigurationsAsync(environment, application);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating configurations for {Environment}/{Application}", environment, application);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("snapshots")]
    public async Task<ActionResult<ConfigurationSnapshot>> CreateSnapshot([FromBody] CreateSnapshotDto dto)
    {
        try
        {
            var snapshot = await _configurationService.CreateSnapshotAsync(dto);
            return CreatedAtAction(nameof(GetSnapshots), 
                new { environment = snapshot.Environment, application = snapshot.Application }, 
                snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating configuration snapshot");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("snapshots/{snapshotId}/restore")]
    public async Task<ActionResult> RestoreFromSnapshot(Guid snapshotId)
    {
        try
        {
            var result = await _configurationService.RestoreFromSnapshotAsync(snapshotId);
            if (!result)
                return NotFound($"Snapshot with ID '{snapshotId}' not found");

            return Ok("Configuration restored successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring from snapshot {SnapshotId}", snapshotId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("snapshots")]
    public async Task<ActionResult<List<ConfigurationSnapshot>>> GetSnapshots(
        [FromQuery] string environment,
        [FromQuery] string application)
    {
        try
        {
            var snapshots = await _configurationService.GetSnapshotsAsync(environment, application);
            return Ok(snapshots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving snapshots for {Environment}/{Application}", environment, application);
            return StatusCode(500, "Internal server error");
        }
    }
}
