using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Analytics.Domain.Models;

namespace Arif.Platform.Analytics.Domain.Interfaces;

public interface IAdvancedAnalyticsService
{
    Task<RealTimeMetrics> GetRealTimeMetricsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<PredictiveAnalytics> GeneratePredictiveAnalyticsAsync(string metricType, TimeSpan forecastPeriod, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<AnomalyDetection>> DetectAnomaliesAsync(string dataSource, DateTime startDate, DateTime endDate, Guid tenantId, CancellationToken cancellationToken = default);
    Task<CustomerInsights> GenerateCustomerInsightsAsync(Guid customerId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<BusinessIntelligenceReport> GenerateBusinessIntelligenceReportAsync(ReportConfiguration config, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<KPIMetric>> GetAdvancedKPIsAsync(List<string> kpiTypes, DateTime startDate, DateTime endDate, Guid tenantId, CancellationToken cancellationToken = default);
    Task<SentimentAnalysisResult> AnalyzeSentimentTrendsAsync(DateTime startDate, DateTime endDate, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ConversationAnalytics> AnalyzeConversationPatternsAsync(DateTime startDate, DateTime endDate, Guid tenantId, CancellationToken cancellationToken = default);
    Task<UserBehaviorAnalytics> AnalyzeUserBehaviorAsync(DateTime startDate, DateTime endDate, Guid tenantId, CancellationToken cancellationToken = default);
    Task<PerformanceOptimizationSuggestions> GetOptimizationSuggestionsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<object> GetConversationAnalyticsAsync(object request);
    Task<object> GetUserEngagementMetricsAsync(object request);
    Task<object> GetBotPerformanceTrackingAsync(object request);
    Task<object> GetPredictiveInsightsAsync(object request);
    Task<object> GetAnomalyDetectionAsync(object request);
    Task<object> TrackEventAsync(object request);
    Task<object> GetAnalyticsReportAsync(string reportId, Guid tenantId);
    Task<object> GenerateCustomReportAsync(object request);
}
