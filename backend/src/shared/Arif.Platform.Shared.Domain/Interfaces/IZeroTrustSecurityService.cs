using Arif.Platform.Shared.Domain.Entities;

namespace Arif.Platform.Shared.Domain.Interfaces;

public interface IZeroTrustSecurityService
{
    Task<TrustScore> EvaluateTrustScoreAsync(TrustEvaluationRequest request, CancellationToken cancellationToken = default);
    Task<DeviceFingerprint> GenerateDeviceFingerprintAsync(DeviceFingerprintRequest request, CancellationToken cancellationToken = default);
    Task<AccessDecision> MakeAccessDecisionAsync(AccessRequest request, CancellationToken cancellationToken = default);
    Task<bool> ValidateDeviceAsync(string deviceId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<SecurityRisk>> AnalyzeSecurityRisksAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> StartContinuousMonitoringAsync(Guid userId, string sessionId, CancellationToken cancellationToken = default);
    Task<bool> StopContinuousMonitoringAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<BehaviorAnalysis> AnalyzeBehaviorPatternsAsync(Guid userId, TimeSpan analysisWindow, CancellationToken cancellationToken = default);
    Task<List<ThreatIntelligence>> GetThreatIntelligenceAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<ComplianceStatus> CheckComplianceStatusAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public class TrustEvaluationRequest
{
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime RequestTime { get; set; }
    public string RequestedResource { get; set; } = string.Empty;
    public Dictionary<string, object> ContextData { get; set; } = new();
}

public class TrustScore
{
    public double Score { get; set; }
    public TrustLevel Level { get; set; }
    public List<TrustFactor> Factors { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
    public TimeSpan ValidFor { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class TrustFactor
{
    public string Name { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Score { get; set; }
    public string Description { get; set; } = string.Empty;
}

public enum TrustLevel
{
    VeryLow = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    VeryHigh = 4
}

public class DeviceFingerprintRequest
{
    public string UserAgent { get; set; } = string.Empty;
    public string ScreenResolution { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public List<string> Plugins { get; set; } = new();
    public Dictionary<string, string> Headers { get; set; } = new();
    public string CanvasFingerprint { get; set; } = string.Empty;
    public string WebGLFingerprint { get; set; } = string.Empty;
}

public class DeviceFingerprint
{
    public string FingerprintId { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsKnownDevice { get; set; }
    public double SimilarityScore { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
}

public class AccessRequest
{
    public Guid UserId { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public TrustScore CurrentTrustScore { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
}

public class AccessDecision
{
    public bool IsAllowed { get; set; }
    public AccessDecisionType DecisionType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<string> RequiredActions { get; set; } = new();
    public TimeSpan ValidFor { get; set; }
    public Dictionary<string, object> Conditions { get; set; } = new();
}

public enum AccessDecisionType
{
    Allow,
    Deny,
    Challenge,
    StepUp,
    Monitor
}



public class BehaviorAnalysis
{
    public Guid UserId { get; set; }
    public Dictionary<string, double> BehaviorMetrics { get; set; } = new();
    public List<BehaviorAnomaly> Anomalies { get; set; } = new();
    public double BaselineDeviation { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public TimeSpan AnalysisWindow { get; set; }
}

public class BehaviorAnomaly
{
    public string AnomalyType { get; set; } = string.Empty;
    public double Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

public class ThreatIntelligence
{
    public string ThreatId { get; set; } = string.Empty;
    public string ThreatType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public ThreatSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public List<string> Indicators { get; set; } = new();
}

public enum ThreatSeverity
{
    Info,
    Low,
    Medium,
    High,
    Critical
}
