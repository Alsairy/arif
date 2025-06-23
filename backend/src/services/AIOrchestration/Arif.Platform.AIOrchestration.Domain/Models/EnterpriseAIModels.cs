using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.AIOrchestration.Domain.Models;

public class VectorSearchResult
{
    public string Query { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
    public List<VectorSearchMatch> Results { get; set; } = new();
    public int TotalResults { get; set; }
    public double SearchTime { get; set; }
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
}

public class VectorSearchMatch
{
    public string DocumentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public double[] Embedding { get; set; } = Array.Empty<double>();
}

public class VectorDocument
{
    public string DocumentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double[] Embedding { get; set; } = Array.Empty<double>();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string CollectionName { get; set; } = string.Empty;
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
}

public class ModelFineTuningResult
{
    public string JobId { get; set; } = string.Empty;
    public string BaseModelId { get; set; } = string.Empty;
    public FineTuningStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime EstimatedCompletionTime { get; set; }
    public int TrainingDataSize { get; set; }
    public string? FineTunedModelId { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> ValidationErrors { get; set; } = new();
}

public enum FineTuningStatus
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}

public class FineTuningDataPoint
{
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public double Weight { get; set; } = 1.0;
}

public class FineTuningConfiguration
{
    public int Epochs { get; set; } = 3;
    public double LearningRate { get; set; } = 0.0001;
    public int BatchSize { get; set; } = 16;
    public double ValidationSplit { get; set; } = 0.2;
    public bool EarlyStopping { get; set; } = true;
    public Dictionary<string, object> HyperParameters { get; set; } = new();
    public string OptimizationObjective { get; set; } = "accuracy";
}

public class ModelVersion
{
    public string ModelId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string VersionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public ModelVersionStatus Status { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string? ParentVersionId { get; set; }
    public List<string> Tags { get; set; } = new();
    public ModelPerformanceMetrics? PerformanceMetrics { get; set; }
}

public enum ModelVersionStatus
{
    Draft,
    Active,
    Deprecated,
    Archived
}

public class ModelPerformanceMetrics
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double AverageLatency { get; set; }
    public double ThroughputPerSecond { get; set; }
    public Dictionary<string, double> CustomMetrics { get; set; } = new();
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
}

public class ABTestResult
{
    public string TestId { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public List<string> ModelVersions { get; set; } = new();
    public ABTestConfiguration Configuration { get; set; } = new();
    public ABTestStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid TenantId { get; set; }
    public Dictionary<string, double> TrafficSplit { get; set; } = new();
    public Dictionary<string, ABTestMetrics> Results { get; set; } = new();
    public string? WinningVersion { get; set; }
}

public enum ABTestStatus
{
    Draft,
    Running,
    Paused,
    Completed,
    Cancelled
}

public class ABTestConfiguration
{
    public string TestName { get; set; } = string.Empty;
    public Dictionary<string, double>? TrafficSplit { get; set; }
    public int MinSampleSize { get; set; } = 1000;
    public double SignificanceLevel { get; set; } = 0.05;
    public TimeSpan MaxDuration { get; set; } = TimeSpan.FromDays(30);
    public List<string> SuccessMetrics { get; set; } = new();
    public Dictionary<string, object> Criteria { get; set; } = new();
}

public class ABTestMetrics
{
    public string ModelVersion { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public double AverageLatency { get; set; }
    public double SuccessRate { get; set; }
    public double UserSatisfactionScore { get; set; }
    public Dictionary<string, double> CustomMetrics { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class ModelPerformanceComparison
{
    public string ComparisonId { get; set; } = string.Empty;
    public List<string> ModelVersions { get; set; } = new();
    public List<string> TestPrompts { get; set; } = new();
    public Dictionary<string, ModelVersionPerformance> Results { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Winner { get; set; }
    public string ComparisonCriteria { get; set; } = "overall_performance";
}

public class ModelVersionPerformance
{
    public string ModelVersion { get; set; } = string.Empty;
    public List<TestResult> TestResults { get; set; } = new();
    public double AverageLatency { get; set; }
    public double SuccessRate { get; set; }
    public int TotalTests { get; set; }
    public Dictionary<string, double> Metrics { get; set; } = new();
}

public class TestResult
{
    public string Prompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public double Latency { get; set; }
    public bool Success { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

public class AIModel
{
    public string ModelId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public ModelStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
    public List<string> SupportedLanguages { get; set; } = new();
    public ModelCapabilities Capabilities { get; set; } = new();
}

public enum ModelStatus
{
    Training,
    Ready,
    Deploying,
    Deployed,
    Failed,
    Deprecated
}

public class ModelCapabilities
{
    public bool SupportsTextGeneration { get; set; }
    public bool SupportsTextClassification { get; set; }
    public bool SupportsQuestionAnswering { get; set; }
    public bool SupportsSummarization { get; set; }
    public bool SupportsTranslation { get; set; }
    public bool SupportsEmbeddings { get; set; }
    public bool SupportsFineTuning { get; set; }
    public List<string> CustomCapabilities { get; set; } = new();
}

public class AIModelPrediction
{
    public string PredictionId { get; set; } = string.Empty;
    public string PredictionType { get; set; } = string.Empty;
    public Dictionary<string, object> Predictions { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string ModelUsed { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class AIModelTrainingResult
{
    public string TrainingJobId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public TrainingStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Dictionary<string, double> TrainingMetrics { get; set; } = new();
    public List<string> ValidationErrors { get; set; } = new();
}

public enum TrainingStatus
{
    Queued,
    Initializing,
    Training,
    Validating,
    Completed,
    Failed,
    Cancelled
}

public class TrainingDataPoint
{
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public double Weight { get; set; } = 1.0;
}

public class ModelConfiguration
{
    public string ModelType { get; set; } = string.Empty;
    public Dictionary<string, object> HyperParameters { get; set; } = new();
    public TrainingConfiguration TrainingConfig { get; set; } = new();
    public ValidationConfiguration ValidationConfig { get; set; } = new();
}

public class TrainingConfiguration
{
    public int Epochs { get; set; } = 10;
    public double LearningRate { get; set; } = 0.001;
    public int BatchSize { get; set; } = 32;
    public string Optimizer { get; set; } = "adam";
    public Dictionary<string, object> OptimizerParams { get; set; } = new();
}

public class ValidationConfiguration
{
    public double ValidationSplit { get; set; } = 0.2;
    public string ValidationStrategy { get; set; } = "holdout";
    public List<string> Metrics { get; set; } = new();
    public bool EarlyStopping { get; set; } = true;
    public int Patience { get; set; } = 5;
}

public enum EnsembleStrategy
{
    Voting,
    Averaging,
    Stacking,
    Boosting,
    Weighted
}

public class AIServiceMetrics
{
    public string ServiceName { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public double AverageLatency { get; set; }
    public double ErrorRate { get; set; }
    public Dictionary<string, int> RequestsByModel { get; set; } = new();
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
}

public class ModelUsageStatistics
{
    public string ModelId { get; set; } = string.Empty;
    public int TotalInferences { get; set; }
    public double AverageLatency { get; set; }
    public double SuccessRate { get; set; }
    public Dictionary<Guid, int> UsageByTenant { get; set; } = new();
    public DateTime LastUsed { get; set; }
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
}

public class AIModelOptimizationSuggestion
{
    public string ModelId { get; set; } = string.Empty;
    public string SuggestionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double PotentialImprovement { get; set; }
    public string ImplementationComplexity { get; set; } = string.Empty;
    public List<string> RequiredActions { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class AIModelPerformanceMetrics
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double Latency { get; set; }
    public double Throughput { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double ErrorRate { get; set; }
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, object> CustomMetrics { get; set; } = new();
}
