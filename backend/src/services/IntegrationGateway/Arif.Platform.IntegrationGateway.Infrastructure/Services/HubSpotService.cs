using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Arif.Platform.IntegrationGateway.Domain.Interfaces;
using Arif.Platform.IntegrationGateway.Domain.Models;

namespace Arif.Platform.IntegrationGateway.Infrastructure.Services;

public class HubSpotService : IHubSpotService
{
    private readonly ILogger<HubSpotService> _logger;
    private readonly HttpClient _httpClient;
    private const string HubSpotApiBaseUrl = "https://api.hubapi.com";

    public HubSpotService(ILogger<HubSpotService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> TestConnectionAsync(IntegrationConfiguration config)
    {
        try
        {
            var hubspotConfig = JsonSerializer.Deserialize<HubSpotConfiguration>(
                JsonSerializer.Serialize(config.Settings));
            
            if (hubspotConfig == null) return false;

            var url = $"{HubSpotApiBaseUrl}/account-info/v3/details";
            
            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(hubspotConfig.AccessToken))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {hubspotConfig.AccessToken}");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {hubspotConfig.ApiKey}");
            }

            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test HubSpot connection");
            return false;
        }
    }

    public async Task<string> SendMessageAsync(IntegrationConfiguration config, IntegrationMessage message)
    {
        var hubspotConfig = JsonSerializer.Deserialize<HubSpotConfiguration>(
            JsonSerializer.Serialize(config.Settings));
        
        if (hubspotConfig == null) 
            throw new ArgumentException("Invalid HubSpot configuration");

        var noteData = new Dictionary<string, object>
        {
            {"properties", new Dictionary<string, object>
                {
                    {"hs_note_body", message.Content},
                    {"hs_timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}
                }
            }
        };

        return await CreateNoteAsync(hubspotConfig, noteData);
    }

    public async Task<string> CreateContactAsync(HubSpotConfiguration config, Dictionary<string, object> contactData)
    {
        try
        {
            var url = $"{HubSpotApiBaseUrl}/crm/v3/objects/contacts";

            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(config.AccessToken))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.AccessToken}");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
            }

            var payload = new
            {
                properties = contactData
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var contactId = result.GetProperty("id").GetString();
                
                _logger.LogInformation("HubSpot contact created successfully. ID: {ContactId}", contactId);
                return contactId ?? string.Empty;
            }
            else
            {
                _logger.LogError("Failed to create HubSpot contact. Response: {Response}", responseContent);
                throw new Exception($"HubSpot API error: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create HubSpot contact");
            throw;
        }
    }

    public async Task<string> CreateDealAsync(HubSpotConfiguration config, Dictionary<string, object> dealData)
    {
        try
        {
            var url = $"{HubSpotApiBaseUrl}/crm/v3/objects/deals";

            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(config.AccessToken))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.AccessToken}");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
            }

            var payload = new
            {
                properties = dealData
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var dealId = result.GetProperty("id").GetString();
                
                _logger.LogInformation("HubSpot deal created successfully. ID: {DealId}", dealId);
                return dealId ?? string.Empty;
            }
            else
            {
                _logger.LogError("Failed to create HubSpot deal. Response: {Response}", responseContent);
                throw new Exception($"HubSpot API error: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create HubSpot deal");
            throw;
        }
    }

    public async Task<List<Dictionary<string, object>>> GetContactsAsync(HubSpotConfiguration config, int limit = 100)
    {
        try
        {
            var url = $"{HubSpotApiBaseUrl}/crm/v3/objects/contacts?limit={limit}";

            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(config.AccessToken))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.AccessToken}");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
            }

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var contacts = new List<Dictionary<string, object>>();

                if (result.TryGetProperty("results", out var resultsArray))
                {
                    foreach (var contact in resultsArray.EnumerateArray())
                    {
                        var contactDict = new Dictionary<string, object>();
                        
                        if (contact.TryGetProperty("id", out var id))
                        {
                            contactDict["id"] = id.GetString() ?? string.Empty;
                        }

                        if (contact.TryGetProperty("properties", out var properties))
                        {
                            foreach (var property in properties.EnumerateObject())
                            {
                                contactDict[property.Name] = property.Value.ToString();
                            }
                        }

                        contacts.Add(contactDict);
                    }
                }

                _logger.LogInformation("Retrieved {Count} HubSpot contacts", contacts.Count);
                return contacts;
            }
            else
            {
                _logger.LogError("Failed to get HubSpot contacts. Response: {Response}", responseContent);
                throw new Exception($"HubSpot API error: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get HubSpot contacts");
            throw;
        }
    }

    public async Task<bool> ProcessWebhookAsync(IntegrationConfiguration config, string payload)
    {
        try
        {
            _logger.LogInformation("Processing HubSpot webhook: {Payload}", payload);
            
            var webhookData = JsonSerializer.Deserialize<JsonElement>(payload);
            
            if (webhookData.TryGetProperty("subscriptionType", out var subscriptionType))
            {
                var eventType = subscriptionType.GetString();
                _logger.LogInformation("HubSpot webhook event type: {EventType}", eventType);

                if (webhookData.TryGetProperty("objectId", out var objectId))
                {
                    var id = objectId.GetString();
                    _logger.LogInformation("HubSpot webhook for object ID: {ObjectId}", id);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process HubSpot webhook");
            return false;
        }
    }

    private async Task<string> CreateNoteAsync(HubSpotConfiguration config, Dictionary<string, object> noteData)
    {
        try
        {
            var url = $"{HubSpotApiBaseUrl}/crm/v3/objects/notes";

            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(config.AccessToken))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.AccessToken}");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
            }

            var json = JsonSerializer.Serialize(noteData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var noteId = result.GetProperty("id").GetString();
                
                _logger.LogInformation("HubSpot note created successfully. ID: {NoteId}", noteId);
                return noteId ?? string.Empty;
            }
            else
            {
                _logger.LogError("Failed to create HubSpot note. Response: {Response}", responseContent);
                throw new Exception($"HubSpot API error: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create HubSpot note");
            throw;
        }
    }
}
