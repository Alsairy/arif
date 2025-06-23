using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Arif.Platform.IntegrationGateway.Domain.Interfaces;
using Arif.Platform.IntegrationGateway.Domain.Models;

namespace Arif.Platform.IntegrationGateway.Infrastructure.Services;

public class FacebookService : IFacebookService
{
    private readonly ILogger<FacebookService> _logger;
    private readonly HttpClient _httpClient;
    private const string GraphApiBaseUrl = "https://graph.facebook.com/v18.0";

    public FacebookService(ILogger<FacebookService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> TestConnectionAsync(IntegrationConfiguration config)
    {
        try
        {
            var facebookConfig = JsonSerializer.Deserialize<FacebookConfiguration>(
                JsonSerializer.Serialize(config.Settings));
            
            if (facebookConfig == null) return false;

            var url = $"{GraphApiBaseUrl}/me?access_token={facebookConfig.PageAccessToken}";
            var response = await _httpClient.GetAsync(url);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test Facebook connection");
            return false;
        }
    }

    public async Task<string> SendMessageAsync(IntegrationConfiguration config, IntegrationMessage message)
    {
        var facebookConfig = JsonSerializer.Deserialize<FacebookConfiguration>(
            JsonSerializer.Serialize(config.Settings));
        
        if (facebookConfig == null) 
            throw new ArgumentException("Invalid Facebook configuration");

        return await SendFacebookMessageAsync(facebookConfig, message.To, message.Content);
    }

    public async Task<string> SendFacebookMessageAsync(FacebookConfiguration config, string recipientId, string message)
    {
        try
        {
            var url = $"{GraphApiBaseUrl}/me/messages?access_token={config.PageAccessToken}";
            
            var payload = new
            {
                recipient = new { id = recipientId },
                message = new { text = message }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var messageId = result.GetProperty("message_id").GetString();
                
                _logger.LogInformation("Facebook message sent successfully. Message ID: {MessageId}", messageId);
                return messageId ?? string.Empty;
            }
            else
            {
                _logger.LogError("Failed to send Facebook message. Response: {Response}", responseContent);
                throw new Exception($"Facebook API error: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Facebook message to {RecipientId}", recipientId);
            throw;
        }
    }

    public async Task<bool> ProcessWebhookAsync(IntegrationConfiguration config, string payload)
    {
        var facebookConfig = JsonSerializer.Deserialize<FacebookConfiguration>(
            JsonSerializer.Serialize(config.Settings));
        
        if (facebookConfig == null) return false;

        return await ProcessFacebookWebhookAsync(payload, facebookConfig.WebhookVerifyToken);
    }

    public async Task<bool> ProcessFacebookWebhookAsync(string payload, string verifyToken)
    {
        try
        {
            _logger.LogInformation("Processing Facebook webhook: {Payload}", payload);
            
            var webhookData = JsonSerializer.Deserialize<JsonElement>(payload);
            
            if (webhookData.TryGetProperty("entry", out var entries))
            {
                foreach (var entry in entries.EnumerateArray())
                {
                    if (entry.TryGetProperty("messaging", out var messaging))
                    {
                        foreach (var message in messaging.EnumerateArray())
                        {
                            await ProcessFacebookMessage(message);
                        }
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Facebook webhook");
            return false;
        }
    }

    public async Task<bool> VerifyWebhookAsync(string mode, string token, string challenge, string verifyToken)
    {
        if (mode == "subscribe" && token == verifyToken)
        {
            _logger.LogInformation("Facebook webhook verified successfully");
            return true;
        }
        
        _logger.LogWarning("Facebook webhook verification failed");
        return false;
    }

    private async Task ProcessFacebookMessage(JsonElement message)
    {
        try
        {
            if (message.TryGetProperty("message", out var messageData))
            {
                var senderId = message.GetProperty("sender").GetProperty("id").GetString();
                var text = messageData.TryGetProperty("text", out var textElement) 
                    ? textElement.GetString() 
                    : null;

                if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(senderId))
                {
                    _logger.LogInformation("Received Facebook message from {SenderId}: {Text}", senderId, text);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Facebook message");
        }
    }
}
