using Microsoft.AspNetCore.Mvc;
using Arif.Platform.IntegrationGateway.Domain.Interfaces;
using Arif.Platform.IntegrationGateway.Domain.Models;

namespace Arif.Platform.IntegrationGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly ITwilioService _twilioService;
    private readonly IFacebookService _facebookService;
    private readonly ISlackService _slackService;
    private readonly ISalesforceService _salesforceService;
    private readonly IHubSpotService _hubspotService;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        ITwilioService twilioService,
        IFacebookService facebookService,
        ISlackService slackService,
        ISalesforceService salesforceService,
        IHubSpotService hubspotService,
        ILogger<WebhooksController> logger)
    {
        _twilioService = twilioService;
        _facebookService = facebookService;
        _slackService = slackService;
        _salesforceService = salesforceService;
        _hubspotService = hubspotService;
        _logger = logger;
    }

    [HttpPost("twilio")]
    public async Task<IActionResult> TwilioWebhook()
    {
        try
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            _logger.LogInformation("Received Twilio webhook: {Body}", body);

            var result = await _twilioService.ProcessTwilioWebhookAsync(body);
            
            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to process webhook");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Twilio webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("facebook")]
    public async Task<IActionResult> FacebookWebhookVerification(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.challenge")] string challenge,
        [FromQuery(Name = "hub.verify_token")] string verifyToken)
    {
        try
        {
            var defaultVerifyToken = "arif_platform_verify_token";
            
            var isValid = await _facebookService.VerifyWebhookAsync(mode, verifyToken, challenge, defaultVerifyToken);
            
            if (isValid)
            {
                return Ok(challenge);
            }
            else
            {
                return Forbid();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Facebook webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("facebook")]
    public async Task<IActionResult> FacebookWebhook()
    {
        try
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            _logger.LogInformation("Received Facebook webhook: {Body}", body);

            var result = await _facebookService.ProcessFacebookWebhookAsync(body, "arif_platform_verify_token");
            
            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to process webhook");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Facebook webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("slack")]
    public async Task<IActionResult> SlackWebhook()
    {
        try
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            _logger.LogInformation("Received Slack webhook: {Body}", body);

            var signature = Request.Headers["X-Slack-Signature"].FirstOrDefault() ?? string.Empty;
            var timestamp = Request.Headers["X-Slack-Request-Timestamp"].FirstOrDefault() ?? string.Empty;

            var result = await _slackService.ProcessSlackWebhookAsync(body, signature, timestamp);
            
            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to process webhook");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Slack webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("salesforce")]
    public async Task<IActionResult> SalesforceWebhook()
    {
        try
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            _logger.LogInformation("Received Salesforce webhook: {Body}", body);

            var integrationConfig = new IntegrationConfiguration
            {
                IntegrationType = "Salesforce"
            };

            var result = await _salesforceService.ProcessWebhookAsync(integrationConfig, body);
            
            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to process webhook");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Salesforce webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("hubspot")]
    public async Task<IActionResult> HubSpotWebhook()
    {
        try
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            _logger.LogInformation("Received HubSpot webhook: {Body}", body);

            var integrationConfig = new IntegrationConfiguration
            {
                IntegrationType = "HubSpot"
            };

            var result = await _hubspotService.ProcessWebhookAsync(integrationConfig, body);
            
            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to process webhook");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing HubSpot webhook");
            return StatusCode(500, "Internal server error");
        }
    }
}
