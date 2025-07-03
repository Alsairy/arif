using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arif.Platform.WorkflowEngine.Domain.Interfaces;
using Arif.Platform.WorkflowEngine.Domain.DTOs;
using Arif.Platform.Shared.Infrastructure.Services;

namespace Arif.Platform.WorkflowEngine.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowDefinitionService _workflowDefinitionService;
        private readonly IWorkflowExecutionService _workflowExecutionService;
        private readonly IStateMachineService _stateMachineService;
        private readonly IWorkflowTemplateService _workflowTemplateService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(
            IWorkflowDefinitionService workflowDefinitionService,
            IWorkflowExecutionService workflowExecutionService,
            IStateMachineService stateMachineService,
            IWorkflowTemplateService workflowTemplateService,
            ICurrentUserService currentUserService,
            ILogger<WorkflowController> logger)
        {
            _workflowDefinitionService = workflowDefinitionService;
            _workflowExecutionService = workflowExecutionService;
            _stateMachineService = stateMachineService;
            _workflowTemplateService = workflowTemplateService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        [HttpPost("definitions")]
        public async Task<IActionResult> CreateWorkflowDefinitionAsync([FromBody] CreateWorkflowDefinitionRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { error = "Workflow name is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.CreatedBy = userId.Value;

                _logger.LogInformation("Creating workflow definition {WorkflowName} for user {UserId} in tenant {TenantId}", 
                    request.Name, userId, tenantId);

                var workflowDefinition = await _workflowDefinitionService.CreateWorkflowDefinitionAsync(request);
                return Ok(workflowDefinition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow definition");
                return StatusCode(500, new { error = "An error occurred while creating the workflow definition" });
            }
        }

        [HttpGet("definitions")]
        public async Task<IActionResult> GetWorkflowDefinitionsAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving workflow definitions for tenant {TenantId}", tenantId);

                var request = new GetWorkflowDefinitionsRequest
                {
                    TenantId = tenantId.Value,
                    Page = page,
                    PageSize = pageSize,
                    Search = search
                };

                var workflowDefinitions = await _workflowDefinitionService.GetWorkflowDefinitionsAsync(request);
                return Ok(workflowDefinitions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow definitions");
                return StatusCode(500, new { error = "An error occurred while retrieving workflow definitions" });
            }
        }

        [HttpGet("definitions/{workflowId}")]
        public async Task<IActionResult> GetWorkflowDefinitionAsync(Guid workflowId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving workflow definition {WorkflowId} for tenant {TenantId}", workflowId, tenantId);

                var workflowDefinition = await _workflowDefinitionService.GetWorkflowDefinitionAsync(workflowId, tenantId.Value);
                
                if (workflowDefinition == null)
                {
                    return NotFound(new { error = "Workflow definition not found" });
                }

                return Ok(workflowDefinition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow definition {WorkflowId}", workflowId);
                return StatusCode(500, new { error = "An error occurred while retrieving the workflow definition" });
            }
        }

        [HttpPut("definitions/{workflowId}")]
        public async Task<IActionResult> UpdateWorkflowDefinitionAsync(Guid workflowId, [FromBody] UpdateWorkflowDefinitionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request body is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.WorkflowId = workflowId;
                request.TenantId = tenantId.Value;
                request.UpdatedBy = userId.Value;

                _logger.LogInformation("Updating workflow definition {WorkflowId} for user {UserId} in tenant {TenantId}", 
                    workflowId, userId, tenantId);

                var workflowDefinition = await _workflowDefinitionService.UpdateWorkflowDefinitionAsync(workflowId, request);
                
                if (workflowDefinition == null)
                {
                    return NotFound(new { error = "Workflow definition not found" });
                }

                return Ok(workflowDefinition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow definition {WorkflowId}", workflowId);
                return StatusCode(500, new { error = "An error occurred while updating the workflow definition" });
            }
        }

        [HttpDelete("definitions/{workflowId}")]
        public async Task<IActionResult> DeleteWorkflowDefinitionAsync(Guid workflowId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                _logger.LogInformation("Deleting workflow definition {WorkflowId} for user {UserId} in tenant {TenantId}", 
                    workflowId, userId, tenantId);

                var success = await _workflowDefinitionService.DeleteWorkflowDefinitionAsync(workflowId, tenantId.Value);
                
                if (!success)
                {
                    return NotFound(new { error = "Workflow definition not found" });
                }

                return Ok(new { message = "Workflow definition deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow definition {WorkflowId}", workflowId);
                return StatusCode(500, new { error = "An error occurred while deleting the workflow definition" });
            }
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteWorkflowAsync([FromBody] ExecuteWorkflowRequest request)
        {
            try
            {
                if (request == null || request.WorkflowId == Guid.Empty)
                {
                    return BadRequest(new { error = "WorkflowId is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.ExecutedBy = userId.Value;

                _logger.LogInformation("Executing workflow {WorkflowId} for user {UserId} in tenant {TenantId}", 
                    request.WorkflowId, userId, tenantId);

                var execution = await _workflowExecutionService.ExecuteWorkflowAsync(request.WorkflowId, request);
                return Ok(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing workflow {WorkflowId}", request?.WorkflowId);
                return StatusCode(500, new { error = "An error occurred while executing the workflow" });
            }
        }

        [HttpGet("executions")]
        public async Task<IActionResult> GetWorkflowExecutionsAsync(
            [FromQuery] Guid? workflowId = null,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving workflow executions for tenant {TenantId}", tenantId);

                var request = new GetWorkflowExecutionsRequest
                {
                    TenantId = tenantId.Value,
                    WorkflowId = workflowId,
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                };

                var executions = await _workflowExecutionService.GetWorkflowExecutionsAsync(request);
                return Ok(executions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow executions");
                return StatusCode(500, new { error = "An error occurred while retrieving workflow executions" });
            }
        }

        [HttpGet("executions/{executionId}")]
        public async Task<IActionResult> GetWorkflowExecutionAsync(Guid executionId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving workflow execution {ExecutionId} for tenant {TenantId}", executionId, tenantId);

                var execution = await _workflowExecutionService.GetWorkflowExecutionAsync(executionId, tenantId.Value);
                
                if (execution == null)
                {
                    return NotFound(new { error = "Workflow execution not found" });
                }

                return Ok(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow execution {ExecutionId}", executionId);
                return StatusCode(500, new { error = "An error occurred while retrieving the workflow execution" });
            }
        }

        [HttpPost("executions/{executionId}/cancel")]
        public async Task<IActionResult> CancelWorkflowExecutionAsync(Guid executionId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                _logger.LogInformation("Cancelling workflow execution {ExecutionId} for user {UserId} in tenant {TenantId}", 
                    executionId, userId, tenantId);

                var success = await _workflowExecutionService.CancelWorkflowExecutionAsync(executionId, tenantId);
                
                if (!success)
                {
                    return NotFound(new { error = "Workflow execution not found or cannot be cancelled" });
                }

                return Ok(new { message = "Workflow execution cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling workflow execution {ExecutionId}", executionId);
                return StatusCode(500, new { error = "An error occurred while cancelling the workflow execution" });
            }
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetWorkflowTemplatesAsync()
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving workflow templates for tenant {TenantId}", tenantId);

                var templates = await _workflowTemplateService.GetWorkflowTemplatesAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workflow templates");
                return StatusCode(500, new { error = "An error occurred while retrieving workflow templates" });
            }
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateWorkflowAsync([FromBody] ValidateWorkflowRequest request)
        {
            try
            {
                if (request == null || request.WorkflowDefinition == null)
                {
                    return BadRequest(new { error = "Workflow definition is required" });
                }

                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Validating workflow definition for tenant {TenantId}", tenantId);

                var validation = await _workflowDefinitionService.ValidateWorkflowAsync(request, tenantId);
                return Ok(validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating workflow");
                return StatusCode(500, new { error = "An error occurred while validating the workflow" });
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                service = "Workflow Engine Service",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
