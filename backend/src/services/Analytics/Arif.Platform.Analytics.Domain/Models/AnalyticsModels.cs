using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Analytics.Domain.Models;

public class RealTimeMetrics
{
    public Guid TenantId { get; set; }
    public DateTime Timestamp { get; set; }
    public int ActiveSessions { get; set; }
    public double MessagesPerMinute { get; set; }
    public double AverageResponseTime { get; set; }
    public double UserSatisfactionScore { get; set; }
    public double BotAccuracyRate { get; set; }
    public List<string> TopIntents { get; set; } = new();
    public double ErrorRate { get; set; }
    public double SystemLoad { get; set; }
    public int DatabaseConnections { get; set; }
    public double MemoryUsage { get; set; }
}

public class PredictiveAnalytics
{
    public Guid TenantId { get; set; }
    public TimeSpan ForecastPeriod { get; set; }
    public DateTime GeneratedAt { get; set; }
    public UserGrowthPrediction UserGrowthPrediction { get; set; } = new();
    public VolumeForecasting VolumeForecasting { get; set; } = new();
    public ResourceRequirements ResourceRequirements { get; set; } = new();
    public ChurnRiskAnalysis ChurnRiskAnalysis { get; set; } = new();
    public List<SeasonalTrend> SeasonalTrends { get; set; } = new();
    public PerformanceProjections PerformanceProjections { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
    public double ConfidenceScore { get; set; }
}

public class UserGrowthPrediction
{
    public double PredictedGrowthRate { get; set; }
    public int ExpectedNewUsers { get; set; }
    public ConfidenceInterval ConfidenceInterval { get; set; } = new();
    public Dictionary<string, double> GrowthFactors { get; set; } = new();
}

public class ConfidenceInterval
{
    public double Lower { get; set; }
    public double Upper { get; set; }
    public double Confidence { get; set; } = 0.95;
}

public class VolumeForecasting
{
    public int PredictedVolume { get; set; }
    public List<int> PeakHours { get; set; } = new();
    public Dictionary<string, double> SeasonalFactors { get; set; } = new();
    public List<VolumeProjection> HourlyProjections { get; set; } = new();
}

public class VolumeProjection
{
    public DateTime Hour { get; set; }
    public int ProjectedVolume { get; set; }
    public double Confidence { get; set; }
}

public class ResourceRequirements
{
    public int RecommendedCpuCores { get; set; }
    public int RecommendedMemoryGB { get; set; }
    public int RecommendedStorageGB { get; set; }
    public decimal EstimatedCost { get; set; }
    public Dictionary<string, object> CloudProviderRecommendations { get; set; } = new();
}

public class ChurnRiskAnalysis
{
    public double OverallChurnRisk { get; set; }
    public int HighRiskUsers { get; set; }
    public List<string> ChurnIndicators { get; set; } = new();
    public Dictionary<string, double> RiskFactors { get; set; } = new();
}

public class SeasonalTrend
{
    public string TrendName { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public double Impact { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<TrendDataPoint> DataPoints { get; set; } = new();
}

public class TrendDataPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class PerformanceProjections
{
    public double ProjectedResponseTime { get; set; }
    public double ProjectedAccuracy { get; set; }
    public double ProjectedSatisfaction { get; set; }
    public Dictionary<string, double> MetricProjections { get; set; } = new();
}

public class AnomalyDetectionResult
{
    public Guid TenantId { get; set; }
    public DateRange AnalysisPeriod { get; set; } = new();
    public List<Anomaly> DetectedAnomalies { get; set; } = new();
    public int TotalAnomalies { get; set; }
    public int CriticalAnomalies { get; set; }
    public DateTime AnalysisTimestamp { get; set; }
    public List<string> RecommendedActions { get; set; } = new();
}

public class DateRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan Duration => EndDate - StartDate;
}

public class Anomaly
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public AnomalySeverity Severity { get; set; }
    public DateTime DetectedAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> AffectedMetrics { get; set; } = new();
}

public enum AnomalySeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public class CustomerInsights
{
    public Guid TenantId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public UserSegmentation UserSegmentation { get; set; } = new();
    public BehaviorPatterns BehaviorPatterns { get; set; } = new();
    public SatisfactionAnalysis SatisfactionAnalysis { get; set; } = new();
    public EngagementMetrics EngagementMetrics { get; set; } = new();
    public RetentionAnalysis RetentionAnalysis { get; set; } = new();
    public List<ConversionFunnel> ConversionFunnels { get; set; } = new();
    public List<PersonalizationOpportunity> PersonalizationOpportunities { get; set; } = new();
    public List<string> RecommendedImprovements { get; set; } = new();
}

public class UserSegmentation
{
    public List<UserSegment> Segments { get; set; } = new();
    public Dictionary<string, int> SegmentDistribution { get; set; } = new();
    public List<SegmentCharacteristic> KeyCharacteristics { get; set; } = new();
}

public class UserSegment
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public double Percentage { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
}

public class AnomalyDetection
{
    public string AnomalyId { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public string DataSource { get; set; } = string.Empty;
    public string AnomalyType { get; set; } = string.Empty;
    public double Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
}

public class KPIMetric
{
    public string KPIType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double PreviousValue { get; set; }
    public double PercentageChange { get; set; }
    public string Trend { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class UserBehaviorAnalytics
{
    public Dictionary<string, int> UserActions { get; set; } = new();
    public Dictionary<string, double> EngagementMetrics { get; set; } = new();
    public List<UserJourney> CommonJourneys { get; set; } = new();
    public Dictionary<string, double> DropOffPoints { get; set; } = new();
}

public class PerformanceOptimizationSuggestions
{
    public List<OptimizationSuggestion> Suggestions { get; set; } = new();
    public Dictionary<string, double> PotentialImpact { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class OptimizationSuggestion
{
    public string SuggestionId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public double EstimatedImpact { get; set; }
    public string ImplementationComplexity { get; set; } = string.Empty;
}

public class ReportConfiguration
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> Metrics { get; set; } = new();
    public List<string> Dimensions { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
}

public class SegmentCharacteristic
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Importance { get; set; }
}

public class BehaviorPatterns
{
    public List<Pattern> CommonPatterns { get; set; } = new();
    public Dictionary<string, double> UsagePatterns { get; set; } = new();
    public List<UserJourney> TypicalJourneys { get; set; } = new();
}

public class Pattern
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Frequency { get; set; }
    public List<string> Steps { get; set; } = new();
}

public class UserJourney
{
    public string JourneyName { get; set; } = string.Empty;
    public List<JourneyStep> Steps { get; set; } = new();
    public double CompletionRate { get; set; }
    public TimeSpan AverageDuration { get; set; }
}

public class JourneyStep
{
    public string StepName { get; set; } = string.Empty;
    public int Order { get; set; }
    public double CompletionRate { get; set; }
    public TimeSpan AverageTime { get; set; }
}

public class SatisfactionAnalysis
{
    public double OverallSatisfaction { get; set; }
    public Dictionary<string, double> SatisfactionByCategory { get; set; } = new();
    public List<SatisfactionTrend> Trends { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
}

public class SatisfactionTrend
{
    public DateTime Date { get; set; }
    public double Score { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class EngagementMetrics
{
    public double DailyActiveUsers { get; set; }
    public double WeeklyActiveUsers { get; set; }
    public double MonthlyActiveUsers { get; set; }
    public double AverageSessionDuration { get; set; }
    public double SessionsPerUser { get; set; }
    public Dictionary<string, double> EngagementByFeature { get; set; } = new();
}

public class RetentionAnalysis
{
    public double Day1Retention { get; set; }
    public double Day7Retention { get; set; }
    public double Day30Retention { get; set; }
    public List<RetentionCohort> Cohorts { get; set; } = new();
    public List<string> RetentionFactors { get; set; } = new();
}

public class RetentionCohort
{
    public DateTime CohortDate { get; set; }
    public int InitialUsers { get; set; }
    public Dictionary<int, double> RetentionRates { get; set; } = new();
}

public class ConversionFunnel
{
    public string FunnelName { get; set; } = string.Empty;
    public List<FunnelStep> Steps { get; set; } = new();
    public double OverallConversionRate { get; set; }
    public List<string> DropOffReasons { get; set; } = new();
}

public class FunnelStep
{
    public string StepName { get; set; } = string.Empty;
    public int Order { get; set; }
    public int Users { get; set; }
    public double ConversionRate { get; set; }
    public double DropOffRate { get; set; }
}

public class PersonalizationOpportunity
{
    public string OpportunityName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double PotentialImpact { get; set; }
    public string TargetSegment { get; set; } = string.Empty;
    public List<string> RecommendedActions { get; set; } = new();
}

public class BusinessIntelligenceReport
{
    public Guid TenantId { get; set; }
    public ReportType ReportType { get; set; }
    public DateRange Period { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public ExecutiveSummary ExecutiveSummary { get; set; } = new();
    public Dictionary<string, double> KeyMetrics { get; set; } = new();
    public TrendAnalysis TrendAnalysis { get; set; } = new();
    public PerformanceComparison PerformanceComparison { get; set; } = new();
    public ROIAnalysis ROIAnalysis { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<ChartData> Charts { get; set; } = new();
    public List<string> DataSources { get; set; } = new();
}

public enum ReportType
{
    Executive,
    Operational,
    Financial,
    Performance,
    Customer,
    Technical
}

public class ExecutiveSummary
{
    public string Overview { get; set; } = string.Empty;
    public List<KeyInsight> KeyInsights { get; set; } = new();
    public List<string> Highlights { get; set; } = new();
    public List<string> Concerns { get; set; } = new();
}

public class KeyInsight
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Impact { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class TrendAnalysis
{
    public List<Trend> Trends { get; set; } = new();
    public Dictionary<string, double> TrendStrengths { get; set; } = new();
    public List<string> EmergingTrends { get; set; } = new();
}

public class Trend
{
    public string Name { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public double Strength { get; set; }
    public List<TrendDataPoint> DataPoints { get; set; } = new();
}

public class PerformanceComparison
{
    public Dictionary<string, double> CurrentPeriod { get; set; } = new();
    public Dictionary<string, double> PreviousPeriod { get; set; } = new();
    public Dictionary<string, double> PercentageChanges { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
}

public class ROIAnalysis
{
    public decimal TotalInvestment { get; set; }
    public decimal TotalReturn { get; set; }
    public double ROIPercentage { get; set; }
    public Dictionary<string, decimal> CostBreakdown { get; set; } = new();
    public Dictionary<string, decimal> RevenueBreakdown { get; set; } = new();
}

public class ChartData
{
    public string ChartType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<DataSeries> Series { get; set; } = new();
    public Dictionary<string, object> Options { get; set; } = new();
}

public class DataSeries
{
    public string Name { get; set; } = string.Empty;
    public List<DataPoint> Data { get; set; } = new();
    public string Color { get; set; } = string.Empty;
}

public class DataPoint
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime? Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SentimentAnalysisResult
{
    public Guid TenantId { get; set; }
    public DateRange AnalysisPeriod { get; set; } = new();
    public SentimentScore OverallSentiment { get; set; } = new();
    public Dictionary<string, double> SentimentDistribution { get; set; } = new();
    public List<SentimentTrend> SentimentTrends { get; set; } = new();
    public List<string> TopPositiveTopics { get; set; } = new();
    public List<string> TopNegativeTopics { get; set; } = new();
    public Dictionary<string, SentimentScore> SentimentByChannel { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
    public int AnalyzedConversations { get; set; }
    public DateTime AnalysisTimestamp { get; set; }
}

public class SentimentScore
{
    public double Positive { get; set; }
    public double Neutral { get; set; }
    public double Negative { get; set; }
    public double OverallScore { get; set; }
    public string Sentiment { get; set; } = string.Empty;
}

public class SentimentTrend
{
    public DateTime Date { get; set; }
    public SentimentScore Score { get; set; } = new();
    public int ConversationCount { get; set; }
}

public class ConversationSentiment
{
    public string ConversationId { get; set; } = string.Empty;
    public SentimentScore Sentiment { get; set; } = new();
    public List<string> Topics { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}

public class ConversationAnalytics
{
    public Guid TenantId { get; set; }
    public DateRange AnalysisPeriod { get; set; } = new();
    public int TotalConversations { get; set; }
    public double AverageConversationLength { get; set; }
    public Dictionary<string, int> ConversationsByChannel { get; set; } = new();
    public List<IntentAnalysis> TopIntents { get; set; } = new();
    public double IntentAccuracy { get; set; }
    public List<DropOffPoint> DropOffPoints { get; set; } = new();
    public List<ConversationFlow> ConversationFlows { get; set; } = new();
    public UserJourneyAnalysis UserJourneyAnalysis { get; set; } = new();
    public Dictionary<int, double> PerformanceByTimeOfDay { get; set; } = new();
    public Dictionary<string, double> LanguageDistribution { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class IntentAnalysis
{
    public string IntentName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public double AccuracyRate { get; set; }
    public double AverageConfidence { get; set; }
}

public class DropOffPoint
{
    public string StepName { get; set; } = string.Empty;
    public int DropOffCount { get; set; }
    public double DropOffRate { get; set; }
    public List<string> CommonReasons { get; set; } = new();
}

public class ConversationFlow
{
    public string FlowName { get; set; } = string.Empty;
    public List<FlowStep> Steps { get; set; } = new();
    public double CompletionRate { get; set; }
    public TimeSpan AverageDuration { get; set; }
}

public class FlowStep
{
    public string StepName { get; set; } = string.Empty;
    public int Order { get; set; }
    public double CompletionRate { get; set; }
    public TimeSpan AverageTime { get; set; }
    public List<string> NextSteps { get; set; } = new();
}

public class UserJourneyAnalysis
{
    public List<UserJourney> CommonJourneys { get; set; } = new();
    public Dictionary<string, double> JourneySuccess { get; set; } = new();
    public List<string> OptimizationOpportunities { get; set; } = new();
}

public class HistoricalMetric
{
    public DateTime Timestamp { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class Conversation
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public List<Message> Messages { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class Message
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
