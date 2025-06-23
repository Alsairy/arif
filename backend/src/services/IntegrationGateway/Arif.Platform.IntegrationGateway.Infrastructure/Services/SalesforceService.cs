using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Arif.Platform.IntegrationGateway.Domain.Interfaces;
using Arif.Platform.IntegrationGateway.Domain.Models;

namespace Arif.Platform.IntegrationGateway.Infrastructure.Services;

public class SalesforceService : ISalesforceService
{
    private readonly ILogger<SalesforceService> _logger;
    private readonly HttpClient _httpClient;
    private string? _accessToken;
    private string? _instanceUrl;

    public SalesforceService(ILogger<SalesforceService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> TestConnectionAsync(IntegrationConfiguration config)
    {
        try
        {
            var salesforceConfig = JsonSerializer.Deserialize<SalesforceConfiguration>(
                JsonSerializer.Serialize(config.Settings));
            
            if (salesforceConfig == null) return false;

            var authResult = await AuthenticateAsync(salesforceConfig);
            return authResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test Salesforce connection");
            return false;
        }
    }

    public async Task<string> SendMessageAsync(IntegrationConfiguration config, IntegrationMessage message)
    {
        var salesforceConfig = JsonSerializer.Deserialize<SalesforceConfiguration>(
            JsonSerializer.Serialize(config.Settings));
        
        if (salesforceConfig == null) 
            throw new ArgumentException("Invalid Salesforce configuration");

        var taskData = new Dictionary<string, object>
        {
            {"Subject", "Message from Arif Platform"},
            {"Description", message.Content},
            {"Status", "Open"},
            {"Priority", "Normal"}
        };

        return await CreateRecordAsync(salesforceConfig, "Task", taskData);
    }

    public async Task<string> CreateLeadAsync(SalesforceConfiguration config, Dictionary<string, object> leadData)
    {
        return await CreateRecordAsync(config, "Lead", leadData);
    }

    public async Task<string> CreateContactAsync(SalesforceConfiguration config, Dictionary<string, object> contactData)
    {
        return await CreateRecordAsync(config, "Contact", contactData);
    }

    public async Task<List<Dictionary<string, object>>> QueryRecordsAsync(SalesforceConfiguration config, string soqlQuery)
    {
        try
        {
            await EnsureAuthenticatedAsync(config);

            var encodedQuery = Uri.EscapeDataString(soqlQuery);
            var url = $"{_instanceUrl}/services/data/v58.0/query/?q={encodedQuery}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var records = new List<Dictionary<string, object>>();

                if (result.TryGetProperty("records", out var recordsArray))
                {
                    foreach (var record in recordsArray.EnumerateArray())
                    {
                        var recordDict = new Dictionary<string, object>();
                        foreach (var property in record.EnumerateObject())
                        {
                            if (property.Name != "attributes")
                            {
                                recordDict[property.Name] = property.Value.ToString();
                            }
                        }
                        records.Add(recordDict);
                    }
                }

                _logger.LogInformation("Salesforce query executed successfully. Records returned: {Count}", records.Count);
                return records;
            }
            else
            {
                _logger.LogError("Failed to execute Salesforce query. Response: {Response}", responseContent);
                throw new Exception($"Salesforce API error: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Salesforce query: {Query}", soqlQuery);
            throw;
        }
    }

    public async Task<bool> ProcessWebhookAsync(IntegrationConfiguration config, string payload)
    {
        try
        {
            _logger.LogInformation("Processing Salesforce webhook: {Payload}", payload);
            
            var webhookData = JsonSerializer.Deserialize<JsonElement>(payload);
            
            if (webhookData.TryGetProperty("sobject", out var sobject))
            {
                var objectType = sobject.GetProperty("type").GetString();
                var objectId = sobject.GetProperty("id").GetString();
                
                _logger.LogInformation("Salesforce webhook for {ObjectType} with ID {ObjectId}", objectType, objectId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Salesforce webhook");
            return false;
        }
    }

    private async Task<string> CreateRecordAsync(SalesforceConfiguration config, string objectType, Dictionary<string, object> recordData)
    {
        try
        {
            await EnsureAuthenticatedAsync(config);

            var url = $"{_instanceUrl}/services/data/v58.0/sobjects/{objectType}/";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

            var json = JsonSerializer.Serialize(recordData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var recordId = result.GetProperty("id").GetString();
                
                _logger.LogInformation("Salesforce {ObjectType} created successfully. ID: {RecordId}", objectType, recordId);
                return recordId ?? string.Empty;
            }
            else
            {
                _logger.LogError("Failed to create Salesforce {ObjectType}. Response: {Response}", objectType, responseContent);
                throw new Exception($"Salesforce API error: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Salesforce {ObjectType}", objectType);
            throw;
        }
    }

    private async Task<bool> AuthenticateAsync(SalesforceConfiguration config)
    {
        try
        {
            var url = $"{config.InstanceUrl}/services/oauth2/token";

            var payload = new Dictionary<string, string>
            {
                {"grant_type", "password"},
                {"client_id", config.ClientId},
                {"client_secret", config.ClientSecret},
                {"username", config.Username},
                {"password", config.Password + config.SecurityToken}
            };

            var content = new FormUrlEncodedContent(payload);
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                _accessToken = result.GetProperty("access_token").GetString();
                _instanceUrl = result.GetProperty("instance_url").GetString();
                
                _logger.LogInformation("Salesforce authentication successful");
                return true;
            }
            else
            {
                _logger.LogError("Salesforce authentication failed. Response: {Response}", responseContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate with Salesforce");
            return false;
        }
    }

    private async Task EnsureAuthenticatedAsync(SalesforceConfiguration config)
    {
        if (string.IsNullOrEmpty(_accessToken) || string.IsNullOrEmpty(_instanceUrl))
        {
            var authenticated = await AuthenticateAsync(config);
            if (!authenticated)
            {
                throw new Exception("Failed to authenticate with Salesforce");
            }
        }
    }
}
