using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arif.Platform.Notification.Domain.Interfaces;
using Arif.Platform.Notification.Domain.DTOs;
using Arif.Platform.Shared.Infrastructure.Services;

namespace Arif.Platform.Notification.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ITemplateService _templateService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            IEmailService emailService,
            ISmsService smsService,
            IPushNotificationService pushNotificationService,
            ITemplateService templateService,
            ICurrentUserService currentUserService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _emailService = emailService;
            _smsService = smsService;
            _pushNotificationService = pushNotificationService;
            _templateService = templateService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotificationAsync([FromBody] SendNotificationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "Message is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.SentBy = userId.Value;

                _logger.LogInformation("Sending notification via {Channel} for tenant {TenantId}", 
                    request.Channel, tenantId);

                var notification = await _notificationService.SendNotificationAsync(request);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                return StatusCode(500, new { error = "An error occurred while sending the notification" });
            }
        }

        [HttpPost("email/send")]
        public async Task<IActionResult> SendEmailAsync([FromBody] SendEmailRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.To))
                {
                    return BadRequest(new { error = "Recipient email is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.SentBy = userId.Value;

                _logger.LogInformation("Sending email to {Recipient} for tenant {TenantId}", 
                    request.To, tenantId);

                var result = await _emailService.SendEmailAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                return StatusCode(500, new { error = "An error occurred while sending the email" });
            }
        }

        [HttpPost("sms/send")]
        public async Task<IActionResult> SendSmsAsync([FromBody] SendSmsRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return BadRequest(new { error = "Phone number is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.SentBy = userId.Value;

                _logger.LogInformation("Sending SMS to {PhoneNumber} for tenant {TenantId}", 
                    request.PhoneNumber, tenantId);

                var result = await _smsService.SendSmsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS");
                return StatusCode(500, new { error = "An error occurred while sending the SMS" });
            }
        }

        [HttpPost("push/send")]
        public async Task<IActionResult> SendPushNotificationAsync([FromBody] SendPushNotificationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.DeviceToken))
                {
                    return BadRequest(new { error = "Device token is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.SentBy = userId.Value;

                _logger.LogInformation("Sending push notification to device for tenant {TenantId}", tenantId);

                var result = await _pushNotificationService.SendPushNotificationAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification");
                return StatusCode(500, new { error = "An error occurred while sending the push notification" });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetNotificationHistoryAsync(
            [FromQuery] string? channel = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                var request = new GetNotificationHistoryRequest
                {
                    TenantId = tenantId.Value,
                    Channel = channel,
                    Status = status,
                    StartDate = startDate,
                    EndDate = endDate,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation("Retrieving notification history for tenant {TenantId}", tenantId);

                var history = await _notificationService.GetNotificationHistoryAsync(request);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification history");
                return StatusCode(500, new { error = "An error occurred while retrieving notification history" });
            }
        }

        [HttpGet("{notificationId}")]
        public async Task<IActionResult> GetNotificationAsync(Guid notificationId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving notification {NotificationId} for tenant {TenantId}", 
                    notificationId, tenantId);

                var notification = await _notificationService.GetNotificationAsync(notificationId, tenantId.Value);
                
                if (notification == null)
                {
                    return NotFound(new { error = "Notification not found" });
                }

                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification {NotificationId}", notificationId);
                return StatusCode(500, new { error = "An error occurred while retrieving the notification" });
            }
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplatesAsync()
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving notification templates for tenant {TenantId}", tenantId);

                var templates = await _templateService.GetTemplatesAsync(tenantId.Value);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification templates");
                return StatusCode(500, new { error = "An error occurred while retrieving notification templates" });
            }
        }

        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplateAsync([FromBody] CreateTemplateRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { error = "Template name is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TenantId = tenantId.Value;
                request.CreatedBy = userId.Value;

                _logger.LogInformation("Creating notification template {TemplateName} for tenant {TenantId}", 
                    request.Name, tenantId);

                var template = await _templateService.CreateTemplateAsync(request);
                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification template");
                return StatusCode(500, new { error = "An error occurred while creating the notification template" });
            }
        }

        [HttpGet("templates/{templateId}")]
        public async Task<IActionResult> GetTemplateAsync(Guid templateId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Retrieving notification template {TemplateId} for tenant {TenantId}", 
                    templateId, tenantId);

                var template = await _templateService.GetTemplateAsync(templateId, tenantId.Value);
                
                if (template == null)
                {
                    return NotFound(new { error = "Template not found" });
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification template {TemplateId}", templateId);
                return StatusCode(500, new { error = "An error occurred while retrieving the notification template" });
            }
        }

        [HttpPut("templates/{templateId}")]
        public async Task<IActionResult> UpdateTemplateAsync(Guid templateId, [FromBody] UpdateTemplateRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request body is required" });
                }

                var tenantId = _currentUserService.TenantId;
                var userId = _currentUserService.UserId;

                request.TemplateId = templateId;
                request.TenantId = tenantId.Value;
                request.UpdatedBy = userId.Value;

                _logger.LogInformation("Updating notification template {TemplateId} for tenant {TenantId}", 
                    templateId, tenantId);

                var template = await _templateService.UpdateTemplateAsync(request);
                
                if (template == null)
                {
                    return NotFound(new { error = "Template not found" });
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification template {TemplateId}", templateId);
                return StatusCode(500, new { error = "An error occurred while updating the notification template" });
            }
        }

        [HttpDelete("templates/{templateId}")]
        public async Task<IActionResult> DeleteTemplateAsync(Guid templateId)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                _logger.LogInformation("Deleting notification template {TemplateId} for tenant {TenantId}", 
                    templateId, tenantId);

                var success = await _templateService.DeleteTemplateAsync(templateId, tenantId.Value);
                
                if (!success)
                {
                    return NotFound(new { error = "Template not found" });
                }

                return Ok(new { message = "Template deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification template {TemplateId}", templateId);
                return StatusCode(500, new { error = "An error occurred while deleting the notification template" });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetNotificationStatisticsAsync(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var tenantId = _currentUserService.TenantId;

                var request = new GetNotificationStatisticsRequest
                {
                    TenantId = tenantId.Value,
                    StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                    EndDate = endDate ?? DateTime.UtcNow
                };

                _logger.LogInformation("Retrieving notification statistics for tenant {TenantId}", tenantId);

                var statistics = await _notificationService.GetNotificationStatisticsAsync(request);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification statistics");
                return StatusCode(500, new { error = "An error occurred while retrieving notification statistics" });
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                service = "Notification Service",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
