using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Analytics.Domain.DTOs
{
    public class ConversationAnalyticsRequest
    {
        public Guid TenantId { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public string? BotId { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 50;
    }

    public class ConversationAnalyticsResponse
    {
        public ConversationMetrics Metrics { get; set; } = new();
        
        public ConversationTrend[] Trends { get; set; } = Array.Empty<ConversationTrend>();
        
        public ConversationSummary[] TopConversations { get; set; } = Array.Empty<ConversationSummary>();
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class ConversationMetrics
    {
        public int TotalConversations { get; set; }
        
        public int ActiveConversations { get; set; }
        
        public double AverageResponseTime { get; set; }
        
        public double SatisfactionScore { get; set; }
        
        public int ResolvedConversations { get; set; }
        
        public int EscalatedConversations { get; set; }
        
        public Dictionary<string, int> LanguageDistribution { get; set; } = new();
    }

    public class ConversationTrend
    {
        public DateTime Date { get; set; }
        
        public int ConversationCount { get; set; }
        
        public double AverageResponseTime { get; set; }
        
        public double SatisfactionScore { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ConversationSummary
    {
        public string ConversationId { get; set; } = string.Empty;
        
        public string UserId { get; set; } = string.Empty;
        
        public string BotId { get; set; } = string.Empty;
        
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public int MessageCount { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public double? SatisfactionScore { get; set; }
        
        public string Language { get; set; } = string.Empty;
    }

    public class UserEngagementRequest
    {
        public Guid TenantId { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public string? UserId { get; set; }
    }

    public class UserEngagementResponse
    {
        public EngagementMetrics Metrics { get; set; } = new();
        
        public UserEngagementTrend[] Trends { get; set; } = Array.Empty<UserEngagementTrend>();
        
        public TopUser[] TopUsers { get; set; } = Array.Empty<TopUser>();
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class EngagementMetrics
    {
        public int TotalUsers { get; set; }
        
        public int ActiveUsers { get; set; }
        
        public int NewUsers { get; set; }
        
        public int ReturningUsers { get; set; }
        
        public double AverageSessionDuration { get; set; }
        
        public double UserRetentionRate { get; set; }
        
        public Dictionary<string, int> DeviceDistribution { get; set; } = new();
    }

    public class UserEngagementTrend
    {
        public DateTime Date { get; set; }
        
        public int ActiveUsers { get; set; }
        
        public int NewUsers { get; set; }
        
        public double AverageSessionDuration { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class TopUser
    {
        public string UserId { get; set; } = string.Empty;
        
        public string UserName { get; set; } = string.Empty;
        
        public int ConversationCount { get; set; }
        
        public double TotalSessionDuration { get; set; }
        
        public DateTime LastActivity { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class BotPerformanceRequest
    {
        public Guid TenantId { get; set; }
        
        public string? BotId { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
    }

    public class BotPerformanceResponse
    {
        public BotPerformanceMetrics Metrics { get; set; } = new();
        
        public BotPerformanceTrend[] Trends { get; set; } = Array.Empty<BotPerformanceTrend>();
        
        public BotSummary[] BotSummaries { get; set; } = Array.Empty<BotSummary>();
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class BotPerformanceMetrics
    {
        public double AverageResponseTime { get; set; }
        
        public double SuccessRate { get; set; }
        
        public double ErrorRate { get; set; }
        
        public int TotalRequests { get; set; }
        
        public double AverageConfidenceScore { get; set; }
        
        public Dictionary<string, int> IntentDistribution { get; set; } = new();
        
        public Dictionary<string, double> LanguagePerformance { get; set; } = new();
    }

    public class BotPerformanceTrend
    {
        public DateTime Date { get; set; }
        
        public double ResponseTime { get; set; }
        
        public double SuccessRate { get; set; }
        
        public int RequestCount { get; set; }
        
        public double ConfidenceScore { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class BotSummary
    {
        public string BotId { get; set; } = string.Empty;
        
        public string BotName { get; set; } = string.Empty;
        
        public double AverageResponseTime { get; set; }
        
        public double SuccessRate { get; set; }
        
        public int TotalConversations { get; set; }
        
        public DateTime LastActivity { get; set; }
        
        public string Status { get; set; } = string.Empty;
    }

    public class RealTimeDashboardData
    {
        public DashboardMetrics CurrentMetrics { get; set; } = new();
        
        public RealtimeAlert[] Alerts { get; set; } = Array.Empty<RealtimeAlert>();
        
        public SystemHealth SystemHealth { get; set; } = new();
        
        public Dictionary<string, object>? CustomWidgets { get; set; }
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class DashboardMetrics
    {
        public int ActiveConversations { get; set; }
        
        public int OnlineUsers { get; set; }
        
        public double AverageResponseTime { get; set; }
        
        public double SystemLoad { get; set; }
        
        public int ErrorCount { get; set; }
        
        public Dictionary<string, int> LanguageUsage { get; set; } = new();
    }

    public class RealtimeAlert
    {
        public string Id { get; set; } = string.Empty;
        
        public string Type { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        
        public string Severity { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class SystemHealth
    {
        public string Status { get; set; } = string.Empty;
        
        public double CpuUsage { get; set; }
        
        public double MemoryUsage { get; set; }
        
        public double DiskUsage { get; set; }
        
        public ServiceHealth[] Services { get; set; } = Array.Empty<ServiceHealth>();
        
        public DateTime LastCheck { get; set; } = DateTime.UtcNow;
    }

    public class ServiceHealth
    {
        public string ServiceName { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
        
        public double ResponseTime { get; set; }
        
        public DateTime LastCheck { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class PredictiveInsightsRequest
    {
        public Guid TenantId { get; set; }
        
        public string InsightType { get; set; } = string.Empty;
        
        public int ForecastDays { get; set; } = 7;
    }

    public class PredictiveInsightsResponse
    {
        public PredictiveInsight[] Insights { get; set; } = Array.Empty<PredictiveInsight>();
        
        public ForecastData[] Forecasts { get; set; } = Array.Empty<ForecastData>();
        
        public Recommendation[] Recommendations { get; set; } = Array.Empty<Recommendation>();
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class PredictiveInsight
    {
        public string Type { get; set; } = string.Empty;
        
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public double Confidence { get; set; }
        
        public string Impact { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Data { get; set; }
    }

    public class ForecastData
    {
        public DateTime Date { get; set; }
        
        public string Metric { get; set; } = string.Empty;
        
        public double PredictedValue { get; set; }
        
        public double ConfidenceInterval { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class Recommendation
    {
        public string Id { get; set; } = string.Empty;
        
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string Priority { get; set; } = string.Empty;
        
        public string Category { get; set; } = string.Empty;
        
        public Dictionary<string, object>? ActionData { get; set; }
    }

    public class AnomalyDetectionRequest
    {
        public Guid TenantId { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
    }

    public class AnomalyDetectionResponse
    {
        public Anomaly[] Anomalies { get; set; } = Array.Empty<Anomaly>();
        
        public AnomalyPattern[] Patterns { get; set; } = Array.Empty<AnomalyPattern>();
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class Anomaly
    {
        public string Id { get; set; } = string.Empty;
        
        public string Type { get; set; } = string.Empty;
        
        public DateTime DetectedAt { get; set; }
        
        public string Description { get; set; } = string.Empty;
        
        public double Severity { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Data { get; set; }
    }

    public class AnomalyPattern
    {
        public string PatternType { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public double Frequency { get; set; }
        
        public DateTime FirstOccurrence { get; set; }
        
        public DateTime LastOccurrence { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class AnalyticsEventRequest
    {
        [Required]
        public string EventType { get; set; } = string.Empty;
        
        public Guid TenantId { get; set; }
        
        public Guid UserId { get; set; }
        
        public string? SessionId { get; set; }
        
        public string? BotId { get; set; }
        
        public Dictionary<string, object>? Properties { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class CustomReportRequest
    {
        [Required]
        public string ReportName { get; set; } = string.Empty;
        
        public Guid TenantId { get; set; }
        
        public Guid CreatedBy { get; set; }
        
        public string? Description { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public string[] Metrics { get; set; } = Array.Empty<string>();
        
        public Dictionary<string, object>? Filters { get; set; }
        
        public string Format { get; set; } = "json";
    }

    public class CustomReportResponse
    {
        public string ReportId { get; set; } = string.Empty;
        
        public string ReportName { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string? DownloadUrl { get; set; }
        
        public Dictionary<string, object>? ReportData { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
