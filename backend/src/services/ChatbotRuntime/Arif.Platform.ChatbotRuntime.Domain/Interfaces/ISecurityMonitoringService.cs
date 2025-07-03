namespace Arif.Platform.ChatbotRuntime.Domain.Interfaces;

public interface ISecurityMonitoringService
{
    Task LogSecurityEventAsync(string eventType, string sessionId, string details);
    Task<bool> DetectSuspiciousActivityAsync(string sessionId, string userInput);
    Task<SecurityThreatLevel> AssessThreatLevelAsync(string sessionId);
    Task BlockSuspiciousSessionAsync(string sessionId, string reason);
}

public enum SecurityThreatLevel
{
    Low,
    Medium,
    High,
    Critical
}
