using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arif.Platform.AIOrchestration.Domain.Interfaces;
using Arif.Platform.AIOrchestration.Domain.DTOs;
using Arif.Platform.Shared.Infrastructure.Services;

namespace Arif.Platform.AIOrchestration.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AIOrchestrationController : ControllerBase
    {
        private readonly IAdvancedAIService _aiService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AIOrchestrationController> _logger;

        public AIOrchestrationController(
            IAdvancedAIService aiService,
            ICurrentUserService currentUserService,
            ILogger<AIOrchestrationController> logger)
        {
            _aiService = aiService;
            _currentUserService = currentUserService;
            _logger = logger;
        }



        [HttpGet("models")]
        public async Task<IActionResult> GetAvailableModelsAsync()
        {
            try
            {
                var tenantId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException("Tenant ID not found");

                _logger.LogInformation("Retrieving available models for tenant {TenantId}", tenantId);

                var models = await _aiService.GetAvailableModelsAsync(tenantId);

                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available models");
                return StatusCode(500, new { error = "An error occurred while retrieving models" });
            }
        }



        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                service = "AI Orchestration Service",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
