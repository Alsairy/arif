using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arif.Platform.ChatbotRuntime.Domain.Interfaces;
using Arif.Platform.ChatbotRuntime.Domain.DTOs;
using Arif.Platform.Shared.Infrastructure.Services;

namespace Arif.Platform.ChatbotRuntime.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatbotRuntimeController : ControllerBase
    {
        private readonly IChatbotRuntimeService _chatbotRuntimeService;
        private readonly IConversationService _conversationService;
        private readonly ISessionService _sessionService;
        private readonly IChannelService _channelService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ChatbotRuntimeController> _logger;

        public ChatbotRuntimeController(
            IChatbotRuntimeService chatbotRuntimeService,
            IConversationService conversationService,
            ISessionService sessionService,
            IChannelService channelService,
            ICurrentUserService currentUserService,
            ILogger<ChatbotRuntimeController> logger)
        {
            _chatbotRuntimeService = chatbotRuntimeService;
            _conversationService = conversationService;
            _sessionService = sessionService;
            _channelService = channelService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        [HttpPost("chat")]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessChatMessageAsync([FromBody] ChatMessageRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "Message is required" });
                }

                _logger.LogInformation("Processing chat message for session {SessionId} in channel {ChannelId}", 
                    request.SessionId, request.ChannelId);

                var response = await _chatbotRuntimeService.ProcessChatMessageAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                return StatusCode(500, new { error = "An error occurred while processing your message" });
            }
        }

        [HttpPost("sessions")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateSessionAsync([FromBody] CreateSessionRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.ChannelId))
                {
                    return BadRequest(new { error = "ChannelId is required" });
                }

                _logger.LogInformation("Creating new session for channel {ChannelId}", request.ChannelId);

                var session = await _sessionService.CreateSessionAsync(request);
                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session");
                return StatusCode(500, new { error = "An error occurred while creating the session" });
            }
        }

        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSessionAsync(Guid sessionId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving session {SessionId} for tenant {TenantId}", sessionId, tenantId);

                var session = await _sessionService.GetSessionAsync(sessionId, tenantId.Value);
                
                if (session == null)
                {
                    return NotFound(new { error = "Session not found" });
                }

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
                return StatusCode(500, new { error = "An error occurred while retrieving the session" });
            }
        }

        [HttpPost("sessions/{sessionId}/end")]
        public async Task<IActionResult> EndSessionAsync(Guid sessionId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Ending session {SessionId} for tenant {TenantId}", sessionId, tenantId);

                var success = await _sessionService.EndSessionAsync(sessionId, tenantId.Value);
                
                if (!success)
                {
                    return NotFound(new { error = "Session not found or already ended" });
                }

                return Ok(new { message = "Session ended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session {SessionId}", sessionId);
                return StatusCode(500, new { error = "An error occurred while ending the session" });
            }
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversationsAsync(
            [FromQuery] string? sessionId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                var request = new GetConversationsRequest
                {
                    TenantId = tenantId.Value,
                    SessionId = sessionId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieving conversations for tenant {TenantId}", tenantId);

                var conversations = await _conversationService.GetConversationsAsync(request);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversations");
                return StatusCode(500, new { error = "An error occurred while retrieving conversations" });
            }
        }

        [HttpGet("conversations/{conversationId}")]
        public async Task<IActionResult> GetConversationAsync(Guid conversationId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving conversation {ConversationId} for tenant {TenantId}", 
                    conversationId, tenantId);

                var conversation = await _conversationService.GetConversationAsync(conversationId, tenantId.Value);
                
                if (conversation == null)
                {
                    return NotFound(new { error = "Conversation not found" });
                }

                return Ok(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation {ConversationId}", conversationId);
                return StatusCode(500, new { error = "An error occurred while retrieving the conversation" });
            }
        }

        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<IActionResult> GetConversationMessagesAsync(
            Guid conversationId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                var request = new GetConversationMessagesRequest
                {
                    ConversationId = conversationId,
                    TenantId = tenantId.Value,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieving messages for conversation {ConversationId}", conversationId);

                var messages = await _conversationService.GetConversationMessagesAsync(request);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation messages for {ConversationId}", conversationId);
                return StatusCode(500, new { error = "An error occurred while retrieving conversation messages" });
            }
        }

        [HttpGet("channels")]
        public async Task<IActionResult> GetChannelsAsync()
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving channels for tenant {TenantId}", tenantId);

                var channels = await _channelService.GetChannelsAsync(tenantId.Value);
                return Ok(channels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving channels");
                return StatusCode(500, new { error = "An error occurred while retrieving channels" });
            }
        }

        [HttpPost("channels")]
        public async Task<IActionResult> CreateChannelAsync([FromBody] CreateChannelRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { error = "Channel name is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.CreatedBy = userId.Value;

                _logger.LogInformation("Creating channel {ChannelName} for tenant {TenantId}", 
                    request.Name, tenantId);

                var channel = await _channelService.CreateChannelAsync(request);
                return Ok(channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating channel");
                return StatusCode(500, new { error = "An error occurred while creating the channel" });
            }
        }

        [HttpGet("channels/{channelId}")]
        public async Task<IActionResult> GetChannelAsync(Guid channelId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving channel {ChannelId} for tenant {TenantId}", channelId, tenantId);

                var channel = await _channelService.GetChannelAsync(channelId, tenantId.Value);
                
                if (channel == null)
                {
                    return NotFound(new { error = "Channel not found" });
                }

                return Ok(channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving channel {ChannelId}", channelId);
                return StatusCode(500, new { error = "An error occurred while retrieving the channel" });
            }
        }

        [HttpPut("channels/{channelId}")]
        public async Task<IActionResult> UpdateChannelAsync(Guid channelId, [FromBody] UpdateChannelRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request body is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.ChannelId = channelId;
                request.TenantId = tenantId.Value;
                request.UpdatedBy = userId.Value;

                _logger.LogInformation("Updating channel {ChannelId} for tenant {TenantId}", channelId, tenantId);

                var channel = await _channelService.UpdateChannelAsync(request);
                
                if (channel == null)
                {
                    return NotFound(new { error = "Channel not found" });
                }

                return Ok(channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating channel {ChannelId}", channelId);
                return StatusCode(500, new { error = "An error occurred while updating the channel" });
            }
        }

        [HttpDelete("channels/{channelId}")]
        public async Task<IActionResult> DeleteChannelAsync(Guid channelId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Deleting channel {ChannelId} for tenant {TenantId}", channelId, tenantId);

                var success = await _channelService.DeleteChannelAsync(channelId, tenantId.Value);
                
                if (!success)
                {
                    return NotFound(new { error = "Channel not found" });
                }

                return Ok(new { message = "Channel deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting channel {ChannelId}", channelId);
                return StatusCode(500, new { error = "An error occurred while deleting the channel" });
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                service = "Chatbot Runtime Service",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
