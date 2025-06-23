using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Arif.Platform.AIOrchestration.Domain.Interfaces;
using Arif.Platform.AIOrchestration.Domain.Models;
using Arif.Platform.Shared.Common.Security;
using Microsoft.Extensions.Caching.Memory;

namespace Arif.Platform.AIOrchestration.Infrastructure.Services;

public class AdvancedAIService : IAdvancedAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdvancedAIService> _logger;
    private readonly IAuditLogger _auditLogger;
    private readonly IMemoryCache _memoryCache;
    private readonly string _openAIApiKey;
    private readonly string _azureOpenAIEndpoint;
    private readonly Dictionary<string, ModelVersion> _modelVersions;
    private readonly Dictionary<string, ABTestConfiguration> _abTestConfigurations;

    public AdvancedAIService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AdvancedAIService> logger,
        IAuditLogger auditLogger,
        IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _auditLogger = auditLogger;
        _memoryCache = memoryCache;
        _openAIApiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
        _azureOpenAIEndpoint = configuration["AzureOpenAI:Endpoint"] ?? "https://api.openai.com/v1";
        _modelVersions = new Dictionary<string, ModelVersion>();
        _abTestConfigurations = new Dictionary<string, ABTestConfiguration>();
    }

    public async Task<string> ProcessWithGPT4Async(string prompt, string context, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing GPT-4 request for tenant {TenantId}", tenantId);

            var requestBody = new
            {
                model = "gpt-4-turbo-preview",
                messages = new[]
                {
                    new { role = "system", content = context },
                    new { role = "user", content = prompt }
                },
                max_tokens = 4000,
                temperature = 0.7,
                top_p = 1.0,
                frequency_penalty = 0.0,
                presence_penalty = 0.0
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAIApiKey}");

            var response = await _httpClient.PostAsync($"{_azureOpenAIEndpoint}/chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var result = responseData.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                "GPT-4 processing completed",
                Guid.Empty,
                tenantId,
                new { Model = "gpt-4-turbo-preview", TokensUsed = responseData.GetProperty("usage").GetProperty("total_tokens").GetInt32() });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GPT-4 request for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<string> ProcessWithCustomModelAsync(string modelId, string prompt, Dictionary<string, object> parameters, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing custom model {ModelId} request for tenant {TenantId}", modelId, tenantId);

            var requestBody = new
            {
                model_id = modelId,
                prompt = prompt,
                parameters = parameters,
                tenant_id = tenantId.ToString()
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var customModelEndpoint = _configuration["CustomModels:Endpoint"] ?? "http://localhost:8080";
            var response = await _httpClient.PostAsync($"{customModelEndpoint}/api/v1/inference", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var result = responseData.GetProperty("result").GetString() ?? "";

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                "Custom model processing completed",
                Guid.Empty,
                tenantId,
                new { ModelId = modelId, Parameters = parameters });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing custom model {ModelId} request for tenant {TenantId}", modelId, tenantId);
            throw;
        }
    }

    public async Task<AIModelPrediction> GetPredictiveInsightsAsync(string dataType, Dictionary<string, object> historicalData, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating predictive insights for data type {DataType} for tenant {TenantId}", dataType, tenantId);

            var requestBody = new
            {
                data_type = dataType,
                historical_data = historicalData,
                tenant_id = tenantId.ToString(),
                prediction_horizon = 30
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var mlEndpoint = _configuration["MachineLearning:Endpoint"] ?? "http://localhost:8081";
            var response = await _httpClient.PostAsync($"{mlEndpoint}/api/v1/predict", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var prediction = new AIModelPrediction
            {
                PredictionId = Guid.NewGuid().ToString(),
                PredictionType = dataType,
                Predictions = JsonSerializer.Deserialize<Dictionary<string, object>>(responseData.GetProperty("predictions").GetRawText()) ?? new(),
                ConfidenceScore = responseData.GetProperty("confidence").GetDouble(),
                GeneratedAt = DateTime.UtcNow,
                ModelUsed = responseData.GetProperty("model_used").GetString() ?? "unknown"
            };

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                "Predictive insights generated",
                Guid.Empty,
                tenantId,
                new { DataType = dataType, ConfidenceScore = prediction.ConfidenceScore });

            return prediction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating predictive insights for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<string> ProcessMultimodalInputAsync(string textInput, byte[]? imageData, byte[]? audioData, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing multimodal input for tenant {TenantId}", tenantId);

            using var formContent = new MultipartFormDataContent();
            formContent.Add(new StringContent(textInput), "text");
            formContent.Add(new StringContent(tenantId.ToString()), "tenant_id");

            if (imageData != null)
            {
                formContent.Add(new ByteArrayContent(imageData), "image", "image.jpg");
            }

            if (audioData != null)
            {
                formContent.Add(new ByteArrayContent(audioData), "audio", "audio.wav");
            }

            var multimodalEndpoint = _configuration["Multimodal:Endpoint"] ?? "http://localhost:8082";
            var response = await _httpClient.PostAsync($"{multimodalEndpoint}/api/v1/process", formContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var result = responseData.GetProperty("result").GetString() ?? "";

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                "Multimodal processing completed",
                Guid.Empty,
                tenantId,
                new { HasImage = imageData != null, HasAudio = audioData != null });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing multimodal input for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<AIModelTrainingResult> TrainCustomModelAsync(string modelName, List<TrainingDataPoint> trainingData, ModelConfiguration config, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting custom model training {ModelName} for tenant {TenantId}", modelName, tenantId);

            var requestBody = new
            {
                model_name = modelName,
                training_data = trainingData,
                configuration = config,
                tenant_id = tenantId.ToString()
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var trainingEndpoint = _configuration["ModelTraining:Endpoint"] ?? "http://localhost:8083";
            var response = await _httpClient.PostAsync($"{trainingEndpoint}/api/v1/train", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var result = new AIModelTrainingResult
            {
                TrainingJobId = responseData.GetProperty("job_id").GetString() ?? "",
                ModelId = responseData.GetProperty("model_id").GetString() ?? "",
                Status = Enum.Parse<TrainingStatus>(responseData.GetProperty("status").GetString() ?? "Queued"),
                StartedAt = DateTime.UtcNow
            };

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                "Model training started",
                Guid.Empty,
                tenantId,
                new { ModelName = modelName, TrainingJobId = result.TrainingJobId });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting model training for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<List<AIModel>> GetAvailableModelsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var modelsEndpoint = _configuration["ModelRegistry:Endpoint"] ?? "http://localhost:8084";
            var response = await _httpClient.GetAsync($"{modelsEndpoint}/api/v1/models?tenant_id={tenantId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var models = JsonSerializer.Deserialize<List<AIModel>>(responseContent) ?? new List<AIModel>();

            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available models for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<AIModelPerformanceMetrics> GetModelPerformanceAsync(string modelId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var metricsEndpoint = _configuration["ModelMetrics:Endpoint"] ?? "http://localhost:8085";
            var response = await _httpClient.GetAsync($"{metricsEndpoint}/api/v1/metrics/{modelId}?tenant_id={tenantId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var metrics = JsonSerializer.Deserialize<AIModelPerformanceMetrics>(responseContent) ?? new AIModelPerformanceMetrics();

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving model performance for {ModelId} and tenant {TenantId}", modelId, tenantId);
            throw;
        }
    }

    public async Task<string> ProcessWithEnsembleAsync(List<string> modelIds, string prompt, EnsembleStrategy strategy, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing ensemble request with {ModelCount} models for tenant {TenantId}", modelIds.Count, tenantId);

            var requestBody = new
            {
                model_ids = modelIds,
                prompt = prompt,
                strategy = strategy.ToString(),
                tenant_id = tenantId.ToString()
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var ensembleEndpoint = _configuration["EnsembleModels:Endpoint"] ?? "http://localhost:8086";
            var response = await _httpClient.PostAsync($"{ensembleEndpoint}/api/v1/ensemble", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var result = responseData.GetProperty("result").GetString() ?? "";

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                "Ensemble processing completed",
                Guid.Empty,
                tenantId,
                new { ModelIds = modelIds, Strategy = strategy.ToString() });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ensemble request for tenant {TenantId}", tenantId);
            throw;
        }
    }

    #region Enterprise AI/ML Enhancement Services

    public async Task<VectorSearchResult> PerformSemanticSearchAsync(string query, string collectionName, int topK, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing semantic search for tenant {TenantId} in collection {CollectionName}", tenantId, collectionName);

            var queryEmbedding = await GenerateEmbeddingAsync(query, tenantId, cancellationToken);

            var requestBody = new
            {
                collection_name = collectionName,
                query_vector = queryEmbedding,
                top_k = topK,
                tenant_id = tenantId.ToString(),
                include_metadata = true
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var vectorDbEndpoint = _configuration["VectorDatabase:Endpoint"] ?? "http://localhost:8090";
            var response = await _httpClient.PostAsync($"{vectorDbEndpoint}/api/v1/search", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var searchResult = new VectorSearchResult
            {
                Query = query,
                CollectionName = collectionName,
                Results = JsonSerializer.Deserialize<List<VectorSearchMatch>>(responseData.GetProperty("matches").GetRawText()) ?? new(),
                TotalResults = responseData.GetProperty("total_count").GetInt32(),
                SearchTime = responseData.GetProperty("search_time_ms").GetDouble()
            };

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                "Semantic search completed",
                Guid.Empty,
                tenantId,
                new { CollectionName = collectionName, ResultCount = searchResult.Results.Count });

            return searchResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing semantic search for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> IndexDocumentAsync(string documentId, string content, Dictionary<string, object> metadata, string collectionName, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Indexing document {DocumentId} for tenant {TenantId}", documentId, tenantId);

            var embedding = await GenerateEmbeddingAsync(content, tenantId, cancellationToken);

            var requestBody = new
            {
                document_id = documentId,
                content = content,
                embedding = embedding,
                metadata = metadata,
                collection_name = collectionName,
                tenant_id = tenantId.ToString()
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content_body = new StringContent(json, Encoding.UTF8, "application/json");

            var vectorDbEndpoint = _configuration["VectorDatabase:Endpoint"] ?? "http://localhost:8090";
            var response = await _httpClient.PostAsync($"{vectorDbEndpoint}/api/v1/index", content_body, cancellationToken);

            var success = response.IsSuccessStatusCode;
            
            if (success)
            {
                await _auditLogger.LogSecurityEventAsync(
                    SecurityEventType.ConfigurationChange,
                    "Document indexed successfully",
                    Guid.Empty,
                    tenantId,
                    new { DocumentId = documentId, CollectionName = collectionName });
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document {DocumentId} for tenant {TenantId}", documentId, tenantId);
            return false;
        }
    }

    public async Task<ModelFineTuningResult> StartFineTuningAsync(string baseModelId, List<FineTuningDataPoint> trainingData, FineTuningConfiguration config, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting fine-tuning for base model {BaseModelId} for tenant {TenantId}", baseModelId, tenantId);

            var requestBody = new
            {
                base_model_id = baseModelId,
                training_data = trainingData,
                configuration = config,
                tenant_id = tenantId.ToString(),
                job_name = $"finetune_{baseModelId}_{DateTime.UtcNow:yyyyMMddHHmmss}"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var fineTuningEndpoint = _configuration["FineTuning:Endpoint"] ?? "http://localhost:8091";
            var response = await _httpClient.PostAsync($"{fineTuningEndpoint}/api/v1/finetune", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var result = new ModelFineTuningResult
            {
                JobId = responseData.GetProperty("job_id").GetString() ?? "",
                BaseModelId = baseModelId,
                Status = Enum.Parse<FineTuningStatus>(responseData.GetProperty("status").GetString() ?? "Queued"),
                StartedAt = DateTime.UtcNow,
                EstimatedCompletionTime = DateTime.UtcNow.AddHours(responseData.GetProperty("estimated_hours").GetDouble()),
                TrainingDataSize = trainingData.Count
            };

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                "Fine-tuning job started",
                Guid.Empty,
                tenantId,
                new { BaseModelId = baseModelId, JobId = result.JobId });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting fine-tuning for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<ModelVersion> CreateModelVersionAsync(string modelId, string version, Dictionary<string, object> metadata, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating model version {Version} for model {ModelId} for tenant {TenantId}", version, modelId, tenantId);

            var modelVersion = new ModelVersion
            {
                ModelId = modelId,
                Version = version,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = tenantId,
                Metadata = metadata,
                Status = ModelVersionStatus.Active,
                VersionId = Guid.NewGuid().ToString()
            };

            var versionKey = $"{modelId}:{version}";
            _modelVersions[versionKey] = modelVersion;

            var requestBody = new
            {
                model_id = modelId,
                version = version,
                metadata = metadata,
                tenant_id = tenantId.ToString(),
                version_id = modelVersion.VersionId
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var modelRegistryEndpoint = _configuration["ModelRegistry:Endpoint"] ?? "http://localhost:8084";
            var response = await _httpClient.PostAsync($"{modelRegistryEndpoint}/api/v1/versions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                "Model version created",
                Guid.Empty,
                tenantId,
                new { ModelId = modelId, Version = version, VersionId = modelVersion.VersionId });

            return modelVersion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating model version for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<ABTestResult> StartABTestAsync(string testName, List<string> modelVersions, ABTestConfiguration config, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting A/B test {TestName} for tenant {TenantId}", testName, tenantId);

            var abTest = new ABTestResult
            {
                TestId = Guid.NewGuid().ToString(),
                TestName = testName,
                ModelVersions = modelVersions,
                Configuration = config,
                Status = ABTestStatus.Running,
                StartedAt = DateTime.UtcNow,
                TenantId = tenantId,
                TrafficSplit = config.TrafficSplit ?? new Dictionary<string, double>()
            };

            _abTestConfigurations[abTest.TestId] = config;

            var requestBody = new
            {
                test_id = abTest.TestId,
                test_name = testName,
                model_versions = modelVersions,
                configuration = config,
                tenant_id = tenantId.ToString()
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var experimentEndpoint = _configuration["ExperimentTracking:Endpoint"] ?? "http://localhost:8092";
            var response = await _httpClient.PostAsync($"{experimentEndpoint}/api/v1/experiments", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                "A/B test started",
                Guid.Empty,
                tenantId,
                new { TestName = testName, TestId = abTest.TestId, ModelVersions = modelVersions });

            return abTest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting A/B test for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<string> ProcessWithABTestAsync(string testId, string prompt, Dictionary<string, object> context, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing request with A/B test {TestId} for tenant {TenantId}", testId, tenantId);

            if (!_abTestConfigurations.ContainsKey(testId))
            {
                throw new InvalidOperationException($"A/B test {testId} not found");
            }

            var config = _abTestConfigurations[testId];
            
            var selectedModelVersion = SelectModelVersionForABTest(testId, config);

            var result = await ProcessWithModelVersionAsync(selectedModelVersion, prompt, context, tenantId, cancellationToken);

            await RecordABTestInteractionAsync(testId, selectedModelVersion, prompt, result, tenantId, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing A/B test request for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<ModelPerformanceComparison> CompareModelVersionsAsync(List<string> modelVersions, List<string> testPrompts, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Comparing {VersionCount} model versions for tenant {TenantId}", modelVersions.Count, tenantId);

            var comparison = new ModelPerformanceComparison
            {
                ModelVersions = modelVersions,
                ComparisonId = Guid.NewGuid().ToString(),
                StartedAt = DateTime.UtcNow,
                TestPrompts = testPrompts,
                Results = new Dictionary<string, ModelVersionPerformance>()
            };

            foreach (var modelVersion in modelVersions)
            {
                var performance = await EvaluateModelVersionPerformanceAsync(modelVersion, testPrompts, tenantId, cancellationToken);
                comparison.Results[modelVersion] = performance;
            }

            comparison.CompletedAt = DateTime.UtcNow;
            comparison.Winner = DetermineWinningModelVersion(comparison.Results);

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                "Model version comparison completed",
                Guid.Empty,
                tenantId,
                new { ComparisonId = comparison.ComparisonId, Winner = comparison.Winner });

            return comparison;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing model versions for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<List<ModelVersion>> GetModelVersionsAsync(string modelId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var modelRegistryEndpoint = _configuration["ModelRegistry:Endpoint"] ?? "http://localhost:8084";
            var response = await _httpClient.GetAsync($"{modelRegistryEndpoint}/api/v1/models/{modelId}/versions?tenant_id={tenantId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var versions = JsonSerializer.Deserialize<List<ModelVersion>>(responseContent) ?? new List<ModelVersion>();

            return versions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving model versions for {ModelId} and tenant {TenantId}", modelId, tenantId);
            throw;
        }
    }

    public async Task<bool> PromoteModelVersionAsync(string modelId, string version, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Promoting model version {Version} for model {ModelId} for tenant {TenantId}", version, modelId, tenantId);

            var requestBody = new
            {
                model_id = modelId,
                version = version,
                tenant_id = tenantId.ToString(),
                promoted_at = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var modelRegistryEndpoint = _configuration["ModelRegistry:Endpoint"] ?? "http://localhost:8084";
            var response = await _httpClient.PostAsync($"{modelRegistryEndpoint}/api/v1/models/{modelId}/versions/{version}/promote", content, cancellationToken);

            var success = response.IsSuccessStatusCode;

            if (success)
            {
                await _auditLogger.LogSecurityEventAsync(
                    SecurityEventType.ConfigurationChange,
                    "Model version promoted",
                    Guid.Empty,
                    tenantId,
                    new { ModelId = modelId, Version = version });
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error promoting model version for tenant {TenantId}", tenantId);
            return false;
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<double[]> GenerateEmbeddingAsync(string text, Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                input = text,
                model = "text-embedding-ada-002"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAIApiKey}");

            var response = await _httpClient.PostAsync($"{_azureOpenAIEndpoint}/embeddings", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var embeddingArray = responseData.GetProperty("data")[0].GetProperty("embedding");
            var embedding = new double[embeddingArray.GetArrayLength()];
            
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] = embeddingArray[i].GetDouble();
            }

            return embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for tenant {TenantId}", tenantId);
            throw;
        }
    }

    private string SelectModelVersionForABTest(string testId, ABTestConfiguration config)
    {
        var random = new Random();
        var randomValue = random.NextDouble();
        
        double cumulativeWeight = 0;
        foreach (var split in config.TrafficSplit ?? new Dictionary<string, double>())
        {
            cumulativeWeight += split.Value;
            if (randomValue <= cumulativeWeight)
            {
                return split.Key;
            }
        }

        return config.TrafficSplit?.Keys.FirstOrDefault() ?? "";
    }

    private async Task<string> ProcessWithModelVersionAsync(string modelVersion, string prompt, Dictionary<string, object> context, Guid tenantId, CancellationToken cancellationToken)
    {
        return await ProcessWithCustomModelAsync(modelVersion, prompt, context, tenantId, cancellationToken);
    }

    private async Task RecordABTestInteractionAsync(string testId, string modelVersion, string prompt, string result, Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var interaction = new
            {
                test_id = testId,
                model_version = modelVersion,
                prompt_hash = prompt.GetHashCode().ToString(),
                result_length = result.Length,
                tenant_id = tenantId.ToString(),
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(interaction);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var experimentEndpoint = _configuration["ExperimentTracking:Endpoint"] ?? "http://localhost:8092";
            await _httpClient.PostAsync($"{experimentEndpoint}/api/v1/interactions", content, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record A/B test interaction");
        }
    }

    private async Task<ModelVersionPerformance> EvaluateModelVersionPerformanceAsync(string modelVersion, List<string> testPrompts, Guid tenantId, CancellationToken cancellationToken)
    {
        var performance = new ModelVersionPerformance
        {
            ModelVersion = modelVersion,
            TestResults = new List<TestResult>()
        };

        var totalLatency = 0.0;
        var successCount = 0;

        foreach (var prompt in testPrompts)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var result = await ProcessWithModelVersionAsync(modelVersion, prompt, new Dictionary<string, object>(), tenantId, cancellationToken);
                var endTime = DateTime.UtcNow;

                var testResult = new TestResult
                {
                    Prompt = prompt,
                    Response = result,
                    Latency = (endTime - startTime).TotalMilliseconds,
                    Success = !string.IsNullOrEmpty(result)
                };

                performance.TestResults.Add(testResult);
                totalLatency += testResult.Latency;
                if (testResult.Success) successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Test failed for model version {ModelVersion}", modelVersion);
                performance.TestResults.Add(new TestResult
                {
                    Prompt = prompt,
                    Response = "",
                    Latency = 0,
                    Success = false
                });
            }
        }

        performance.AverageLatency = totalLatency / testPrompts.Count;
        performance.SuccessRate = (double)successCount / testPrompts.Count;
        performance.TotalTests = testPrompts.Count;

        return performance;
    }

    private string DetermineWinningModelVersion(Dictionary<string, ModelVersionPerformance> results)
    {
        var bestScore = 0.0;
        var winner = "";

        foreach (var result in results)
        {
            var score = result.Value.SuccessRate * 0.7 + (1.0 / (result.Value.AverageLatency + 1)) * 0.3;
            if (score > bestScore)
            {
                bestScore = score;
                winner = result.Key;
            }
        }

        return winner;
    }

    #endregion
}
