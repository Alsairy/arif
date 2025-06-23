using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
using Arif.Platform.IntegrationGateway.Domain.Interfaces;
using Arif.Platform.IntegrationGateway.Domain.Models;

namespace Arif.Platform.IntegrationGateway.Infrastructure.Services;

public class SlackService : ISlackService
{
    private readonly ILogger<SlackService> _logger;
    private readonly HttpClient _httpClient;
    private const string SlackApiBaseUrl = "https://slack.com/api";

    public SlackService(ILogger<SlackService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> TestConnectionAsync(IntegrationConfiguration config)
    {
        try
        {
            var slackConfig = JsonSerializer.Deserialize<SlackConfiguration>(
                JsonSerializer.Serialize(config.Settings));
            
            if (slackConfig == null) return false;

            var url = $"{SlackApiBaseUrl}/auth.test";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {slackConfig.BotToken}");

            var response = await _httpClient.PostAsync(url, null);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                return result.GetProperty("ok").GetBoolean();
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test Slack connection");
            return false;
        }
    }

    public async Task<string> SendMessageAsync(IntegrationConfiguration config, IntegrationMessage message)
    {
        var slackConfig = JsonSerializer.Deserialize<SlackConfiguration>(
            JsonSerializer.Serialize(config.Settings));
        
        if (slackConfig == null) 
            throw new ArgumentException("Invalid Slack configuration");

        return await SendSlackMessageAsync(slackConfig, message.To, message.Content);
    }

    public async Task<string> SendSlackMessageAsync(SlackConfiguration config, string channel, string message)
    {
        try
        {
            var url = $"{SlackApiBaseUrl}/chat.postMessage";
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.BotToken}");

            var payload = new
            {
                channel = channel,
                text = message,
                as_user = true
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                if (result.GetProperty("ok").GetBoolean())
                {
                    var timestamp = result.GetProperty("ts").GetString();
                    _logger.LogInformation("Slack message sent successfully. Timestamp: {Timestamp}", timestamp);
                    return timestamp ?? string.Empty;
                }
                else
                {
                    var error = result.GetProperty("error").GetString();
                    throw new Exception($"Slack API error: {error}");
                }
            }
            else
            {
                _logger.LogError("Failed to send Slack message. Response: {Response}", responseContent);
                throw new Exception($"Slack API HTTP error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Slack message to channel {Channel}", channel);
            throw;
        }
    }

    public async Task<bool> ProcessWebhookAsync(IntegrationConfiguration config, string payload)
    {
        var slackConfig = JsonSerializer.Deserialize<SlackConfiguration>(
            JsonSerializer.Serialize(config.Settings));
        
        if (slackConfig == null) return false;

        return await ProcessSlackWebhookAsync(payload, string.Empty, string.Empty);
    }

    public async Task<bool> ProcessSlackWebhookAsync(string payload, string signature, string timestamp)
    {
        try
        {
            _logger.LogInformation("Processing Slack webhook: {Payload}", payload);
            
            var webhookData = JsonSerializer.Deserialize<JsonElement>(payload);
            
            if (webhookData.TryGetProperty("challenge", out var challenge))
            {
                _logger.LogInformation("Slack webhook challenge received");
                return true;
            }

            if (webhookData.TryGetProperty("event", out var eventData))
            {
                await ProcessSlackEvent(eventData);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Slack webhook");
            return false;
        }
    }

    public async Task<string> HandleSlackOAuthAsync(string code, SlackConfiguration config)
    {
        try
        {
            var url = $"{SlackApiBaseUrl}/oauth.v2.access";
            
            var payload = new Dictionary<string, string>
            {
                {"client_id", config.ClientId},
                {"client_secret", config.ClientSecret},
                {"code", code}
            };

            var content = new FormUrlEncodedContent(payload);
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                if (result.GetProperty("ok").GetBoolean())
                {
                    var accessToken = result.GetProperty("access_token").GetString();
                    _logger.LogInformation("Slack OAuth completed successfully");
                    return accessToken ?? string.Empty;
                }
                else
                {
                    var error = result.GetProperty("error").GetString();
                    throw new Exception($"Slack OAuth error: {error}");
                }
            }
            else
            {
                throw new Exception($"Slack OAuth HTTP error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle Slack OAuth");
            throw;
        }
    }

    private async Task ProcessSlackEvent(JsonElement eventData)
    {
        try
        {
            var eventType = eventData.GetProperty("type").GetString();
            
            switch (eventType)
            {
                case "message":
                    await ProcessSlackMessage(eventData);
                    break;
                case "app_mention":
                    await ProcessSlackMention(eventData);
                    break;
                default:
                    _logger.LogInformation("Unhandled Slack event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Slack event");
        }
    }

    private async Task ProcessSlackMessage(JsonElement messageData)
    {
        try
        {
            var user = messageData.GetProperty("user").GetString();
            var text = messageData.GetProperty("text").GetString();
            var channel = messageData.GetProperty("channel").GetString();

            _logger.LogInformation("Received Slack message from {User} in {Channel}: {Text}", user, channel, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Slack message");
        }
    }

    private async Task ProcessSlackMention(JsonElement mentionData)
    {
        try
        {
            var user = mentionData.GetProperty("user").GetString();
            var text = mentionData.GetProperty("text").GetString();
            var channel = mentionData.GetProperty("channel").GetString();

            _logger.LogInformation("Received Slack mention from {User} in {Channel}: {Text}", user, channel, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Slack mention");
        }
    }

    private bool VerifySlackSignature(string payload, string signature, string timestamp, string signingSecret)
    {
        try
        {
            var baseString = $"v0:{timestamp}:{payload}";
            var hash = new HMACSHA256(Encoding.UTF8.GetBytes(signingSecret));
            var computedHash = hash.ComputeHash(Encoding.UTF8.GetBytes(baseString));
            var computedSignature = $"v0={Convert.ToHexString(computedHash).ToLower()}";
            
            return computedSignature == signature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Slack signature");
            return false;
        }
    }
}
