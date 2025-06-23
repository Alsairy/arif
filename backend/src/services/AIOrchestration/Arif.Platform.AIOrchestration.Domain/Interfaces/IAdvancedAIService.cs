using Arif.Platform.AIOrchestration.Domain.Models;

namespace Arif.Platform.AIOrchestration.Domain.Interfaces;

public interface IAdvancedAIService
{
    Task<string> ProcessWithGPT4Async(string prompt, string context, Guid tenantId, CancellationToken cancellationToken = default);
    Task<string> ProcessWithCustomModelAsync(string modelId, string prompt, Dictionary<string, object> parameters, Guid tenantId, CancellationToken cancellationToken = default);
    Task<AIModelPrediction> GetPredictiveInsightsAsync(string dataType, Dictionary<string, object> historicalData, Guid tenantId, CancellationToken cancellationToken = default);
    Task<string> ProcessMultimodalInputAsync(string textInput, byte[]? imageData, byte[]? audioData, Guid tenantId, CancellationToken cancellationToken = default);
    Task<AIModelTrainingResult> TrainCustomModelAsync(string modelName, List<TrainingDataPoint> trainingData, ModelConfiguration config, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<AIModel>> GetAvailableModelsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<AIModelPerformanceMetrics> GetModelPerformanceAsync(string modelId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<string> ProcessWithEnsembleAsync(List<string> modelIds, string prompt, EnsembleStrategy strategy, Guid tenantId, CancellationToken cancellationToken = default);

    Task<VectorSearchResult> PerformSemanticSearchAsync(string query, string collectionName, int topK, Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> IndexDocumentAsync(string documentId, string content, Dictionary<string, object> metadata, string collectionName, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ModelFineTuningResult> StartFineTuningAsync(string baseModelId, List<FineTuningDataPoint> trainingData, FineTuningConfiguration config, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ModelVersion> CreateModelVersionAsync(string modelId, string version, Dictionary<string, object> metadata, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ABTestResult> StartABTestAsync(string testName, List<string> modelVersions, ABTestConfiguration config, Guid tenantId, CancellationToken cancellationToken = default);
    Task<string> ProcessWithABTestAsync(string testId, string prompt, Dictionary<string, object> context, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ModelPerformanceComparison> CompareModelVersionsAsync(List<string> modelVersions, List<string> testPrompts, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<ModelVersion>> GetModelVersionsAsync(string modelId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> PromoteModelVersionAsync(string modelId, string version, Guid tenantId, CancellationToken cancellationToken = default);
}
