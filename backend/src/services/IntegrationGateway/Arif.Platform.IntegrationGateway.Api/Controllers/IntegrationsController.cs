using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Arif.Platform.IntegrationGateway.Domain.Interfaces;
using Arif.Platform.IntegrationGateway.Domain.Models;
using Arif.Platform.Shared.Infrastructure.Services;

namespace Arif.Platform.IntegrationGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IntegrationsController : ControllerBase
{
    private readonly ITwilioService _twilioService;
    private readonly IFacebookService _facebookService;
    private readonly ISlackService _slackService;
    private readonly ISalesforceService _salesforceService;
    private readonly IHubSpotService _hubspotService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<IntegrationsController> _logger;

    public IntegrationsController(
        ITwilioService twilioService,
        IFacebookService facebookService,
        ISlackService slackService,
        ISalesforceService salesforceService,
        IHubSpotService hubspotService,
        ITenantContext tenantContext,
        ILogger<IntegrationsController> logger)
    {
        _twilioService = twilioService;
        _facebookService = facebookService;
        _slackService = slackService;
        _salesforceService = salesforceService;
        _hubspotService = hubspotService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    [HttpPost("twilio/test")]
    public async Task<IActionResult> TestTwilioConnection([FromBody] TwilioConfiguration config)
    {
        try
        {
            var integrationConfig = new IntegrationConfiguration
            {
                TenantId = _tenantContext.TenantId,
                IntegrationType = "Twilio",
                Settings = new Dictionary<string, string>
                {
                    {"AccountSid", config.AccountSid},
                    {"AuthToken", config.AuthToken},
                    {"PhoneNumber", config.PhoneNumber},
                    {"WhatsAppNumber", config.WhatsAppNumber ?? string.Empty},
                    {"WebhookUrl", config.WebhookUrl}
                }
            };

            var result = await _twilioService.TestConnectionAsync(integrationConfig);
            return Ok(new { success = result, message = result ? "Connection successful" : "Connection failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test Twilio connection");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("twilio/send-sms")]
    public async Task<IActionResult> SendSms([FromBody] SendSmsRequest request)
    {
        try
        {
            var messageSid = await _twilioService.SendSmsAsync(request.Config, request.To, request.Message);
            return Ok(new { success = true, messageSid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("twilio/send-whatsapp")]
    public async Task<IActionResult> SendWhatsApp([FromBody] SendWhatsAppRequest request)
    {
        try
        {
            var messageSid = await _twilioService.SendWhatsAppAsync(request.Config, request.To, request.Message);
            return Ok(new { success = true, messageSid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("facebook/test")]
    public async Task<IActionResult> TestFacebookConnection([FromBody] FacebookConfiguration config)
    {
        try
        {
            var integrationConfig = new IntegrationConfiguration
            {
                TenantId = _tenantContext.TenantId,
                IntegrationType = "Facebook",
                Settings = new Dictionary<string, string>
                {
                    {"AppId", config.AppId},
                    {"AppSecret", config.AppSecret},
                    {"PageAccessToken", config.PageAccessToken},
                    {"PageId", config.PageId},
                    {"WebhookVerifyToken", config.WebhookVerifyToken}
                }
            };

            var result = await _facebookService.TestConnectionAsync(integrationConfig);
            return Ok(new { success = result, message = result ? "Connection successful" : "Connection failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test Facebook connection");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("facebook/send-message")]
    public async Task<IActionResult> SendFacebookMessage([FromBody] SendFacebookMessageRequest request)
    {
        try
        {
            var messageId = await _facebookService.SendFacebookMessageAsync(request.Config, request.RecipientId, request.Message);
            return Ok(new { success = true, messageId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Facebook message");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("slack/test")]
    public async Task<IActionResult> TestSlackConnection([FromBody] SlackConfiguration config)
    {
        try
        {
            var integrationConfig = new IntegrationConfiguration
            {
                TenantId = _tenantContext.TenantId,
                IntegrationType = "Slack",
                Settings = new Dictionary<string, string>
                {
                    {"ClientId", config.ClientId},
                    {"ClientSecret", config.ClientSecret},
                    {"BotToken", config.BotToken},
                    {"SigningSecret", config.SigningSecret},
                    {"WebhookUrl", config.WebhookUrl}
                }
            };

            var result = await _slackService.TestConnectionAsync(integrationConfig);
            return Ok(new { success = result, message = result ? "Connection successful" : "Connection failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test Slack connection");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("slack/send-message")]
    public async Task<IActionResult> SendSlackMessage([FromBody] SendSlackMessageRequest request)
    {
        try
        {
            var timestamp = await _slackService.SendSlackMessageAsync(request.Config, request.Channel, request.Message);
            return Ok(new { success = true, timestamp });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Slack message");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("salesforce/test")]
    public async Task<IActionResult> TestSalesforceConnection([FromBody] SalesforceConfiguration config)
    {
        try
        {
            var integrationConfig = new IntegrationConfiguration
            {
                TenantId = _tenantContext.TenantId,
                IntegrationType = "Salesforce",
                Settings = new Dictionary<string, string>
                {
                    {"ClientId", config.ClientId},
                    {"ClientSecret", config.ClientSecret},
                    {"Username", config.Username},
                    {"Password", config.Password},
                    {"SecurityToken", config.SecurityToken},
                    {"InstanceUrl", config.InstanceUrl}
                }
            };

            var result = await _salesforceService.TestConnectionAsync(integrationConfig);
            return Ok(new { success = result, message = result ? "Connection successful" : "Connection failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test Salesforce connection");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("salesforce/create-lead")]
    public async Task<IActionResult> CreateSalesforceLead([FromBody] CreateSalesforceLeadRequest request)
    {
        try
        {
            var leadId = await _salesforceService.CreateLeadAsync(request.Config, request.LeadData);
            return Ok(new { success = true, leadId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Salesforce lead");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("hubspot/test")]
    public async Task<IActionResult> TestHubSpotConnection([FromBody] HubSpotConfiguration config)
    {
        try
        {
            var integrationConfig = new IntegrationConfiguration
            {
                TenantId = _tenantContext.TenantId,
                IntegrationType = "HubSpot",
                Settings = new Dictionary<string, string>
                {
                    {"ApiKey", config.ApiKey},
                    {"AccessToken", config.AccessToken ?? string.Empty},
                    {"RefreshToken", config.RefreshToken ?? string.Empty},
                    {"PortalId", config.PortalId}
                }
            };

            var result = await _hubspotService.TestConnectionAsync(integrationConfig);
            return Ok(new { success = result, message = result ? "Connection successful" : "Connection failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test HubSpot connection");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("hubspot/create-contact")]
    public async Task<IActionResult> CreateHubSpotContact([FromBody] CreateHubSpotContactRequest request)
    {
        try
        {
            var contactId = await _hubspotService.CreateContactAsync(request.Config, request.ContactData);
            return Ok(new { success = true, contactId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create HubSpot contact");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public class SendSmsRequest
{
    public TwilioConfiguration Config { get; set; } = new();
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SendWhatsAppRequest
{
    public TwilioConfiguration Config { get; set; } = new();
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SendFacebookMessageRequest
{
    public FacebookConfiguration Config { get; set; } = new();
    public string RecipientId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SendSlackMessageRequest
{
    public SlackConfiguration Config { get; set; } = new();
    public string Channel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class CreateSalesforceLeadRequest
{
    public SalesforceConfiguration Config { get; set; } = new();
    public Dictionary<string, object> LeadData { get; set; } = new();
}

public class CreateHubSpotContactRequest
{
    public HubSpotConfiguration Config { get; set; } = new();
    public Dictionary<string, object> ContactData { get; set; } = new();
}
