using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Arif.Platform.ConfigurationManagement.Domain.Interfaces;
using Arif.Platform.ConfigurationManagement.Domain.DTOs;
using Arif.Platform.ConfigurationManagement.Domain.Entities;

namespace Arif.Platform.ConfigurationManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeploymentController : ControllerBase
{
    private readonly IConfigurationDeploymentService _deploymentService;
    private readonly ILogger<DeploymentController> _logger;

    public DeploymentController(
        IConfigurationDeploymentService deploymentService,
        ILogger<DeploymentController> logger)
    {
        _deploymentService = deploymentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ConfigurationDeploymentDto>> CreateDeployment([FromBody] CreateDeploymentDto dto)
    {
        try
        {
            var deployment = await _deploymentService.CreateDeploymentAsync(dto);
            return CreatedAtAction(nameof(GetDeployment), new { id = deployment.Id }, deployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deployment");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConfigurationDeploymentDto>> GetDeployment(Guid id)
    {
        try
        {
            var deployment = await _deploymentService.GetDeploymentAsync(id);
            return Ok(deployment);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Deployment with ID '{id}' not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ConfigurationDeploymentDto>>> GetDeployments(
        [FromQuery] string environment,
        [FromQuery] string application)
    {
        try
        {
            var deployments = await _deploymentService.GetDeploymentsAsync(environment, application);
            return Ok(deployments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployments for {Environment}/{Application}", environment, application);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/execute")]
    public async Task<ActionResult> ExecuteDeployment(Guid id)
    {
        try
        {
            var result = await _deploymentService.ExecuteDeploymentAsync(id);
            if (!result)
                return BadRequest("Deployment execution failed");

            return Ok("Deployment executed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing deployment {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/rollback")]
    public async Task<ActionResult> RollbackDeployment(Guid id, [FromBody] string reason)
    {
        try
        {
            var result = await _deploymentService.RollbackDeploymentAsync(id, reason);
            if (!result)
                return BadRequest("Deployment rollback failed");

            return Ok("Deployment rolled back successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back deployment {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelDeployment(Guid id)
    {
        try
        {
            var result = await _deploymentService.CancelDeploymentAsync(id);
            if (!result)
                return BadRequest("Deployment cancellation failed");

            return Ok("Deployment cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling deployment {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<string>> GetDeploymentStatus(Guid id)
    {
        try
        {
            var status = await _deploymentService.GetDeploymentStatusAsync(id);
            return Ok(status.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment status {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}/items")]
    public async Task<ActionResult<List<ConfigurationDeploymentItem>>> GetDeploymentItems(Guid id)
    {
        try
        {
            var items = await _deploymentService.GetDeploymentItemsAsync(id);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deployment items {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
