using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arif.Platform.LiveAgent.Domain.Interfaces;
using Arif.Platform.LiveAgent.Domain.DTOs;
using Arif.Platform.Shared.Infrastructure.Services;

namespace Arif.Platform.LiveAgent.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LiveAgentController : ControllerBase
    {
        private readonly ILiveAgentService _liveAgentService;
        private readonly IAgentManagementService _agentManagementService;
        private readonly ITicketService _ticketService;
        private readonly IEscalationService _escalationService;
        private readonly IAgentChatService _agentChatService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<LiveAgentController> _logger;

        public LiveAgentController(
            ILiveAgentService liveAgentService,
            IAgentManagementService agentManagementService,
            ITicketService ticketService,
            IEscalationService escalationService,
            IAgentChatService agentChatService,
            ICurrentUserService currentUserService,
            ILogger<LiveAgentController> logger)
        {
            _liveAgentService = liveAgentService;
            _agentManagementService = agentManagementService;
            _ticketService = ticketService;
            _escalationService = escalationService;
            _agentChatService = agentChatService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        [HttpGet("agents")]
        public async Task<IActionResult> GetAgentsAsync(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                var request = new GetAgentsRequest
                {
                    TenantId = tenantId.Value,
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieving agents for tenant {TenantId}", tenantId);

                var agents = await _agentManagementService.GetAgentsAsync(tenantId.Value);
                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agents");
                return StatusCode(500, new { error = "An error occurred while retrieving agents" });
            }
        }

        [HttpPost("agents")]
        public async Task<IActionResult> CreateAgentAsync([FromBody] CreateAgentRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { error = "Agent email is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.CreatedBy = userId.Value;

                _logger.LogInformation("Creating agent {AgentEmail} for tenant {TenantId}", 
                    request.Email, tenantId);

                var agent = await _agentManagementService.CreateAgentAsync(request);
                return Ok(agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating agent");
                return StatusCode(500, new { error = "An error occurred while creating the agent" });
            }
        }

        [HttpGet("agents/{agentId}")]
        public async Task<IActionResult> GetAgentAsync(Guid agentId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving agent {AgentId} for tenant {TenantId}", agentId, tenantId);

                var agent = await _agentManagementService.GetAgentAsync(agentId, tenantId.Value);
                
                if (agent == null)
                {
                    return NotFound(new { error = "Agent not found" });
                }

                return Ok(agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent {AgentId}", agentId);
                return StatusCode(500, new { error = "An error occurred while retrieving the agent" });
            }
        }

        [HttpPut("agents/{agentId}")]
        public async Task<IActionResult> UpdateAgentAsync(Guid agentId, [FromBody] UpdateAgentRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request body is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.AgentId = agentId;
                request.TenantId = tenantId.Value;
                request.UpdatedBy = userId.Value;

                _logger.LogInformation("Updating agent {AgentId} for tenant {TenantId}", agentId, tenantId);

                var agent = await _agentManagementService.UpdateAgentAsync(agentId, request);
                
                if (agent == null)
                {
                    return NotFound(new { error = "Agent not found" });
                }

                return Ok(agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agent {AgentId}", agentId);
                return StatusCode(500, new { error = "An error occurred while updating the agent" });
            }
        }

        [HttpPost("agents/{agentId}/status")]
        public async Task<IActionResult> UpdateAgentStatusAsync(Guid agentId, [FromBody] UpdateAgentStatusRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Status))
                {
                    return BadRequest(new { error = "Status is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.AgentId = agentId;
                request.TenantId = tenantId.Value;
                request.UpdatedBy = userId.Value;

                _logger.LogInformation("Updating agent {AgentId} status to {Status} for tenant {TenantId}", 
                    agentId, request.Status, tenantId);

                var success = await _agentManagementService.UpdateAgentStatusAsync(agentId, request);
                
                if (success == null || !(bool)success)
                {
                    return NotFound(new { error = "Agent not found" });
                }

                return Ok(new { message = "Agent status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agent {AgentId} status", agentId);
                return StatusCode(500, new { error = "An error occurred while updating agent status" });
            }
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetTicketsAsync(
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] Guid? assignedAgentId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                var request = new GetTicketsRequest
                {
                    TenantId = tenantId.Value,
                    Status = status,
                    Priority = priority,
                    AssignedAgentId = assignedAgentId,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieving tickets for tenant {TenantId}", tenantId);

                var tickets = await _ticketService.GetTicketsAsync(request);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets");
                return StatusCode(500, new { error = "An error occurred while retrieving tickets" });
            }
        }

        [HttpPost("tickets")]
        public async Task<IActionResult> CreateTicketAsync([FromBody] CreateTicketRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Subject))
                {
                    return BadRequest(new { error = "Ticket subject is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.CreatedBy = userId.Value;

                _logger.LogInformation("Creating ticket {TicketSubject} for tenant {TenantId}", 
                    request.Subject, tenantId);

                var ticket = await _ticketService.CreateTicketAsync(request);
                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return StatusCode(500, new { error = "An error occurred while creating the ticket" });
            }
        }

        [HttpGet("tickets/{ticketId}")]
        public async Task<IActionResult> GetTicketAsync(Guid ticketId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving ticket {TicketId} for tenant {TenantId}", ticketId, tenantId);

                var ticket = await _ticketService.GetTicketAsync(ticketId, tenantId.Value);
                
                if (ticket == null)
                {
                    return NotFound(new { error = "Ticket not found" });
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket {TicketId}", ticketId);
                return StatusCode(500, new { error = "An error occurred while retrieving the ticket" });
            }
        }

        [HttpPut("tickets/{ticketId}/assign")]
        public async Task<IActionResult> AssignTicketAsync(Guid ticketId, [FromBody] AssignTicketRequest request)
        {
            try
            {
                if (request == null || request.AgentId == Guid.Empty)
                {
                    return BadRequest(new { error = "Agent ID is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TicketId = ticketId;
                request.TenantId = tenantId.Value;
                request.AssignedBy = userId.Value;

                _logger.LogInformation("Assigning ticket {TicketId} to agent {AgentId} for tenant {TenantId}", 
                    ticketId, request.AgentId, tenantId);

                var result = await _ticketService.AssignTicketAsync(ticketId, request);
                
                if (result == null)
                {
                    return NotFound(new { error = "Ticket or agent not found" });
                }

                return Ok(new { message = "Ticket assigned successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning ticket {TicketId}", ticketId);
                return StatusCode(500, new { error = "An error occurred while assigning the ticket" });
            }
        }

        [HttpPost("tickets/{ticketId}/escalate")]
        public async Task<IActionResult> EscalateTicketAsync(Guid ticketId, [FromBody] EscalateTicketRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest(new { error = "Escalation reason is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TicketId = ticketId;
                request.TenantId = tenantId.Value;
                request.EscalatedBy = userId.Value;

                _logger.LogInformation("Escalating ticket {TicketId} for tenant {TenantId}", ticketId, tenantId);

                var escalation = await _escalationService.EscalateTicketAsync(ticketId, request);
                return Ok(escalation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error escalating ticket {TicketId}", ticketId);
                return StatusCode(500, new { error = "An error occurred while escalating the ticket" });
            }
        }

        [HttpGet("chat/conversations")]
        public async Task<IActionResult> GetAgentConversationsAsync(
            [FromQuery] Guid? agentId = null,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;
                var currentUserId = _currentUserService.UserId;

                var request = new GetAgentConversationsRequest
                {
                    TenantId = tenantId.Value,
                    AgentId = agentId ?? currentUserId.Value,
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieving agent conversations for tenant {TenantId}", tenantId);

                var conversations = await _agentChatService.GetAgentConversationsAsync(request.AgentId, request.TenantId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent conversations");
                return StatusCode(500, new { error = "An error occurred while retrieving agent conversations" });
            }
        }

        [HttpPost("chat/conversations/{conversationId}/join")]
        public async Task<IActionResult> JoinConversationAsync(Guid conversationId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;
                var agentId = _currentUserService.UserId;

                var request = new JoinConversationRequest
                {
                    ConversationId = conversationId,
                    AgentId = agentId.Value,
                    TenantId = tenantId.Value
                };

                _logger.LogInformation("Agent {AgentId} joining conversation {ConversationId} for tenant {TenantId}", 
                    agentId, conversationId, tenantId);

                var result = await _agentChatService.JoinConversationAsync(conversationId, agentId.Value);
                
                if (result == null)
                {
                    return NotFound(new { error = "Conversation not found or already assigned" });
                }

                return Ok(new { message = "Successfully joined conversation" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining conversation {ConversationId}", conversationId);
                return StatusCode(500, new { error = "An error occurred while joining the conversation" });
            }
        }

        [HttpPost("chat/conversations/{conversationId}/messages")]
        public async Task<IActionResult> SendMessageAsync(Guid conversationId, [FromBody] SendAgentMessageRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "Message is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var agentId = _currentUserService.UserId;

                request.ConversationId = conversationId;
                request.AgentId = agentId.Value;
                request.TenantId = tenantId.Value;

                _logger.LogInformation("Agent {AgentId} sending message to conversation {ConversationId}", 
                    agentId, conversationId);

                var message = await _agentChatService.SendMessageAsync(conversationId, request);
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to conversation {ConversationId}", conversationId);
                return StatusCode(500, new { error = "An error occurred while sending the message" });
            }
        }

        [HttpGet("analytics/performance")]
        public async Task<IActionResult> GetAgentPerformanceAsync(
            [FromQuery] Guid? agentId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                var request = new GetAgentPerformanceRequest
                {
                    TenantId = tenantId.Value,
                    AgentId = agentId,
                    StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                    EndDate = endDate ?? DateTime.UtcNow
                };

                _logger.LogInformation("Retrieving agent performance analytics for tenant {TenantId}", tenantId);

                var performance = await _liveAgentService.GetAgentPerformanceAsync(request.AgentId ?? tenantId.Value, request.TenantId);
                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent performance analytics");
                return StatusCode(500, new { error = "An error occurred while retrieving agent performance analytics" });
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                service = "Live Agent Service",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
