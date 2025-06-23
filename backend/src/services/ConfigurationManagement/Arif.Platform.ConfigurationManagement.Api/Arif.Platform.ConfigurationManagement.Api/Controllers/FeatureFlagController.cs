using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Domain.DTOs;

namespace Arif.Platform.ConfigurationManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeatureFlagController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ILogger<FeatureFlagController> _logger;

    public FeatureFlagController(
        IFeatureFlagService featureFlagService,
        ILogger<FeatureFlagController> logger)
    {
        _featureFlagService = featureFlagService;
        _logger = logger;
    }

    [HttpGet("{name}/enabled")]
    public async Task<ActionResult<bool>> IsEnabled(
        string name,
        [FromQuery] string environment,
        [FromQuery] string application,
        [FromQuery] Guid? tenantId = null)
    {
        try
        {
            var context = new Dictionary<string, object>();
            
            if (Request.Headers.ContainsKey("User-Id"))
                context["userId"] = Request.Headers["User-Id"].ToString();
            
            if (Request.Headers.ContainsKey("User-Role"))
                context["userRole"] = Request.Headers["User-Role"].ToString();

            var isEnabled = await _featureFlagService.IsEnabledAsync(name, environment, application, context, tenantId);
            return Ok(isEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature flag {Name}", name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<FeatureFlagDto>> GetFeatureFlag(
        string name,
        [FromQuery] string environment,
        [FromQuery] string application,
        [FromQuery] Guid? tenantId = null)
    {
        try
        {
            var featureFlag = await _featureFlagService.GetFeatureFlagAsync(name, environment, application, tenantId);
            return Ok(featureFlag);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Feature flag '{name}' not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature flag {Name}", name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<FeatureFlagDto>>> GetFeatureFlags(
        [FromQuery] string environment,
        [FromQuery] string application,
        [FromQuery] Guid? tenantId = null)
    {
        try
        {
            var featureFlags = await _featureFlagService.GetFeatureFlagsAsync(environment, application, tenantId);
            return Ok(featureFlags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature flags for {Environment}/{Application}", environment, application);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<FeatureFlagDto>> CreateFeatureFlag([FromBody] CreateFeatureFlagDto dto)
    {
        try
        {
            var featureFlag = await _featureFlagService.CreateFeatureFlagAsync(dto);
            return CreatedAtAction(nameof(GetFeatureFlag), 
                new { name = featureFlag.Name, environment = featureFlag.Environment, application = featureFlag.Application }, 
                featureFlag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feature flag");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FeatureFlagDto>> UpdateFeatureFlag(Guid id, [FromBody] UpdateFeatureFlagDto dto)
    {
        try
        {
            var featureFlag = await _featureFlagService.UpdateFeatureFlagAsync(id, dto);
            return Ok(featureFlag);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Feature flag with ID '{id}' not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feature flag {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFeatureFlag(Guid id)
    {
        try
        {
            var result = await _featureFlagService.DeleteFeatureFlagAsync(id);
            if (!result)
                return NotFound($"Feature flag with ID '{id}' not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feature flag {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/toggle")]
    public async Task<ActionResult> ToggleFeatureFlag(Guid id, [FromBody] bool enabled)
    {
        try
        {
            var result = await _featureFlagService.ToggleFeatureFlagAsync(id, enabled);
            if (!result)
                return NotFound($"Feature flag with ID '{id}' not found");

            return Ok($"Feature flag {(enabled ? "enabled" : "disabled")} successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling feature flag {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/evaluate")]
    public async Task<ActionResult<List<FeatureFlagEvaluationResult>>> EvaluateRules(
        Guid id, 
        [FromBody] Dictionary<string, object> context)
    {
        try
        {
            var results = await _featureFlagService.EvaluateRulesAsync(id, context);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating feature flag rules {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
