using Microsoft.Extensions.Logging;
using System.Text.Json;
using Arif.Platform.IntegrationGateway.Domain.Interfaces;
using Arif.Platform.IntegrationGateway.Domain.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Arif.Platform.IntegrationGateway.Infrastructure.Services;

public class TwilioService : ITwilioService
{
    private readonly ILogger<TwilioService> _logger;
    private readonly HttpClient _httpClient;

    public TwilioService(ILogger<TwilioService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> TestConnectionAsync(IntegrationConfiguration config)
    {
        try
        {
            var twilioConfig = JsonSerializer.Deserialize<TwilioConfiguration>(
                JsonSerializer.Serialize(config.Settings));
            
            if (twilioConfig == null) return false;

            TwilioClient.Init(twilioConfig.AccountSid, twilioConfig.AuthToken);
            
            try
            {
                var account = await Twilio.Rest.Api.V2010.AccountResource.FetchAsync();
                return account != null && account.Status == Twilio.Rest.Api.V2010.AccountResource.StatusEnum.Active;
            }
            catch
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test Twilio connection");
            return false;
        }
    }

    public async Task<string> SendMessageAsync(IntegrationConfiguration config, IntegrationMessage message)
    {
        var twilioConfig = JsonSerializer.Deserialize<TwilioConfiguration>(
            JsonSerializer.Serialize(config.Settings));
        
        if (twilioConfig == null) 
            throw new ArgumentException("Invalid Twilio configuration");

        if (message.MessageType.ToLower() == "whatsapp")
        {
            return await SendWhatsAppAsync(twilioConfig, message.To, message.Content);
        }
        else
        {
            return await SendSmsAsync(twilioConfig, message.To, message.Content);
        }
    }

    public async Task<string> SendSmsAsync(TwilioConfiguration config, string to, string message)
    {
        try
        {
            TwilioClient.Init(config.AccountSid, config.AuthToken);

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(config.PhoneNumber),
                to: new PhoneNumber(to)
            );

            _logger.LogInformation("SMS sent successfully. SID: {MessageSid}", messageResource.Sid);
            return messageResource.Sid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", to);
            throw;
        }
    }

    public async Task<string> SendWhatsAppAsync(TwilioConfiguration config, string to, string message)
    {
        try
        {
            if (string.IsNullOrEmpty(config.WhatsAppNumber))
                throw new ArgumentException("WhatsApp number not configured");

            TwilioClient.Init(config.AccountSid, config.AuthToken);

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber($"whatsapp:{config.WhatsAppNumber}"),
                to: new PhoneNumber($"whatsapp:{to}")
            );

            _logger.LogInformation("WhatsApp message sent successfully. SID: {MessageSid}", messageResource.Sid);
            return messageResource.Sid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {PhoneNumber}", to);
            throw;
        }
    }

    public async Task<bool> ProcessWebhookAsync(IntegrationConfiguration config, string payload)
    {
        return await ProcessTwilioWebhookAsync(payload);
    }

    public async Task<bool> ProcessTwilioWebhookAsync(string payload)
    {
        try
        {
            _logger.LogInformation("Processing Twilio webhook: {Payload}", payload);
            
            var formData = System.Web.HttpUtility.ParseQueryString(payload);
            var messageStatus = formData["MessageStatus"];
            var messageSid = formData["MessageSid"];
            var from = formData["From"];
            var to = formData["To"];
            var body = formData["Body"];

            if (!string.IsNullOrEmpty(body))
            {
                _logger.LogInformation("Received message from {From} to {To}: {Body}", from, to, body);
            }

            if (!string.IsNullOrEmpty(messageStatus))
            {
                _logger.LogInformation("Message {MessageSid} status: {Status}", messageSid, messageStatus);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Twilio webhook");
            return false;
        }
    }
}
