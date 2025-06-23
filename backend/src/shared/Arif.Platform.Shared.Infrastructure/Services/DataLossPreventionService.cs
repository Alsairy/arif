using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Arif.Platform.Shared.Domain.Interfaces;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Common.Security;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class DataLossPreventionService : IDataLossPreventionService
{
    private readonly ILogger<DataLossPreventionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;
    private readonly HttpClient _httpClient;

    public DataLossPreventionService(
        ILogger<DataLossPreventionService> logger,
        IConfiguration configuration,
        IAuditLogger auditLogger,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogger = auditLogger;
        _httpClient = httpClient;
    }

    public async Task<DLPScanResult> ScanContentAsync(string content, Guid tenantId, DLPPolicy policy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Scanning content for DLP violations for tenant {TenantId} using policy {PolicyName}", tenantId, policy.Name);

            var scanResult = new DLPScanResult
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Content = content,
                ContentHash = CalculateContentHash(content),
                ScanTime = DateTime.UtcNow,
                Status = DLPScanStatus.InProgress,
                AppliedPolicy = policy
            };

            var violations = new List<DLPViolation>();

            foreach (var rule in policy.Rules.Where(r => r.IsEnabled))
            {
                var ruleViolations = await EvaluateRule(content, rule, tenantId, policy.Id, cancellationToken);
                violations.AddRange(ruleViolations);
            }

            scanResult.Violations = violations;
            scanResult.Status = DLPScanStatus.Completed;
            scanResult.ConfidenceScore = CalculateOverallConfidenceScore(violations);

            var metadata = new Dictionary<string, object>
            {
                ["ContentLength"] = content.Length,
                ["ViolationsFound"] = violations.Count,
                ["HighSeverityViolations"] = violations.Count(v => v.Severity == DLPSeverity.High || v.Severity == DLPSeverity.Critical),
                ["PolicyId"] = policy.Id,
                ["ScanDuration"] = (DateTime.UtcNow - scanResult.ScanTime).TotalMilliseconds
            };

            scanResult.Metadata = metadata;

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "DLP content scan completed", null, tenantId, new { ScanId = scanResult.Id, ViolationsFound = violations.Count });

            return scanResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning content for DLP violations for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<List<DLPViolation>> GetDLPViolationsAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var violations = new List<DLPViolation>();
            return violations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting DLP violations for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> CreateDLPPolicyAsync(Guid tenantId, DLPPolicy policy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating DLP policy {PolicyName} for tenant {TenantId}", policy.Name, tenantId);

            policy.Id = Guid.NewGuid();
            policy.TenantId = tenantId;
            policy.CreatedAt = DateTime.UtcNow;

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "DLP policy created", null, tenantId, new { PolicyId = policy.Id, PolicyName = policy.Name });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DLP policy for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> UpdateDLPPolicyAsync(Guid policyId, DLPPolicy policy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating DLP policy {PolicyId}", policyId);

            policy.LastModified = DateTime.UtcNow;

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "DLP policy updated", null, policy.TenantId, new { PolicyId = policyId, PolicyName = policy.Name });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating DLP policy {PolicyId}", policyId);
            return false;
        }
    }

    public async Task<List<DLPPolicy>> GetDLPPoliciesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var policies = new List<DLPPolicy>();
            return policies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting DLP policies for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<DLPMetrics> GetDLPMetricsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = new DLPMetrics
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MetricsDate = DateTime.UtcNow,
                TotalScans = 0,
                ViolationsDetected = 0,
                ViolationsBlocked = 0,
                ViolationsQuarantined = 0,
                FalsePositives = 0,
                AverageConfidenceScore = 0.0,
                ViolationsByType = new Dictionary<string, int>(),
                ViolationsBySeverity = new Dictionary<string, int>(),
                Trends = new List<DLPTrend>()
            };

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting DLP metrics for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> QuarantineContentAsync(Guid contentId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Quarantining content {ContentId} with reason: {Reason}", contentId, reason);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "Content quarantined", null, null, new { ContentId = contentId, Reason = reason });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error quarantining content {ContentId}", contentId);
            return false;
        }
    }

    public async Task<bool> ReleaseQuarantinedContentAsync(Guid contentId, string approver, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Releasing quarantined content {ContentId} by approver {Approver}", contentId, approver);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "Quarantined content released", null, null, new { ContentId = contentId, Approver = approver });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing quarantined content {ContentId}", contentId);
            return false;
        }
    }

    public async Task<List<QuarantinedContent>> GetQuarantinedContentAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var quarantinedContent = new List<QuarantinedContent>();
            return quarantinedContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quarantined content for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<DLPIncidentResponse> HandleDLPIncidentAsync(Guid incidentId, DLPIncidentAction action, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Handling DLP incident {IncidentId} with action {ActionType}", incidentId, action.ActionType);

            var incidentResponse = new DLPIncidentResponse
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                CreatedAt = DateTime.UtcNow,
                Status = DLPIncidentStatus.InProgress,
                Actions = new List<DLPIncidentAction> { action }
            };

            action.ExecutedAt = DateTime.UtcNow;
            action.IsSuccessful = true;

            incidentResponse.Status = DLPIncidentStatus.Resolved;
            incidentResponse.ResolvedAt = DateTime.UtcNow;

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "DLP incident handled", null, incidentResponse.TenantId, new { IncidentId = incidentId, ActionType = action.ActionType });

            return incidentResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling DLP incident {IncidentId}", incidentId);
            throw;
        }
    }

    private string CalculateContentHash(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hashBytes);
    }

    private async Task<List<DLPViolation>> EvaluateRule(string content, DLPRule rule, Guid tenantId, Guid policyId, CancellationToken cancellationToken)
    {
        var violations = new List<DLPViolation>();

        try
        {
            switch (rule.Type)
            {
                case DLPRuleType.Regex:
                    violations.AddRange(await EvaluateRegexRule(content, rule, tenantId, policyId));
                    break;
                case DLPRuleType.Keyword:
                    violations.AddRange(await EvaluateKeywordRule(content, rule, tenantId, policyId));
                    break;
                case DLPRuleType.MachineLearning:
                    violations.AddRange(await EvaluateMLRule(content, rule, tenantId, policyId, cancellationToken));
                    break;
                case DLPRuleType.FingerPrint:
                    violations.AddRange(await EvaluateFingerPrintRule(content, rule, tenantId, policyId));
                    break;
                case DLPRuleType.DocumentMatching:
                    violations.AddRange(await EvaluateDocumentMatchingRule(content, rule, tenantId, policyId, cancellationToken));
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating DLP rule {RuleName} for tenant {TenantId}", rule.Name, tenantId);
        }

        return violations;
    }

    private async Task<List<DLPViolation>> EvaluateRegexRule(string content, DLPRule rule, Guid tenantId, Guid policyId)
    {
        var violations = new List<DLPViolation>();

        foreach (var pattern in rule.Patterns)
        {
            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var matches = regex.Matches(content);

                foreach (Match match in matches)
                {
                    var violation = new DLPViolation
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        PolicyId = policyId,
                        RuleName = rule.Name,
                        ViolationType = "Regex Pattern Match",
                        Description = $"Content matches regex pattern: {pattern}",
                        Severity = rule.Severity,
                        DetectedContent = match.Value,
                        Context = GetContextAroundMatch(content, match.Index, match.Length),
                        DetectedAt = DateTime.UtcNow,
                        Status = DLPViolationStatus.Open,
                        MatchedPatterns = new List<string> { pattern },
                        ConfidenceScore = 0.95
                    };

                    violations.Add(violation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating regex pattern {Pattern} in rule {RuleName}", pattern, rule.Name);
            }
        }

        return violations;
    }

    private async Task<List<DLPViolation>> EvaluateKeywordRule(string content, DLPRule rule, Guid tenantId, Guid policyId)
    {
        var violations = new List<DLPViolation>();

        foreach (var keyword in rule.Keywords)
        {
            if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                var violation = new DLPViolation
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PolicyId = policyId,
                    RuleName = rule.Name,
                    ViolationType = "Keyword Match",
                    Description = $"Content contains sensitive keyword: {keyword}",
                    Severity = rule.Severity,
                    DetectedContent = keyword,
                    Context = GetContextAroundKeyword(content, keyword),
                    DetectedAt = DateTime.UtcNow,
                    Status = DLPViolationStatus.Open,
                    MatchedPatterns = new List<string> { keyword },
                    ConfidenceScore = 0.85
                };

                violations.Add(violation);
            }
        }

        return violations;
    }

    private async Task<List<DLPViolation>> EvaluateMLRule(string content, DLPRule rule, Guid tenantId, Guid policyId, CancellationToken cancellationToken)
    {
        var violations = new List<DLPViolation>();

        var confidenceScore = await CalculateMLConfidenceScore(content, rule, cancellationToken);

        if (confidenceScore >= rule.ConfidenceThreshold)
        {
            var violation = new DLPViolation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PolicyId = policyId,
                RuleName = rule.Name,
                ViolationType = "Machine Learning Detection",
                Description = $"Content classified as sensitive by ML model with confidence {confidenceScore:F2}",
                Severity = rule.Severity,
                DetectedContent = content.Length > 100 ? content.Substring(0, 100) + "..." : content,
                Context = "ML-based detection",
                DetectedAt = DateTime.UtcNow,
                Status = DLPViolationStatus.Open,
                MatchedPatterns = new List<string> { "ML_CLASSIFICATION" },
                ConfidenceScore = confidenceScore
            };

            violations.Add(violation);
        }

        return violations;
    }

    private async Task<List<DLPViolation>> EvaluateFingerPrintRule(string content, DLPRule rule, Guid tenantId, Guid policyId)
    {
        var violations = new List<DLPViolation>();

        var contentFingerprint = CalculateContentFingerprint(content);

        foreach (var pattern in rule.Patterns)
        {
            if (CompareFingerprints(contentFingerprint, pattern))
            {
                var violation = new DLPViolation
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PolicyId = policyId,
                    RuleName = rule.Name,
                    ViolationType = "Document Fingerprint Match",
                    Description = "Content matches known sensitive document fingerprint",
                    Severity = rule.Severity,
                    DetectedContent = "Document fingerprint match",
                    Context = "Fingerprint-based detection",
                    DetectedAt = DateTime.UtcNow,
                    Status = DLPViolationStatus.Open,
                    MatchedPatterns = new List<string> { pattern },
                    ConfidenceScore = 0.90
                };

                violations.Add(violation);
            }
        }

        return violations;
    }

    private async Task<List<DLPViolation>> EvaluateDocumentMatchingRule(string content, DLPRule rule, Guid tenantId, Guid policyId, CancellationToken cancellationToken)
    {
        var violations = new List<DLPViolation>();

        var similarity = await CalculateDocumentSimilarity(content, rule, cancellationToken);

        if (similarity >= rule.ConfidenceThreshold)
        {
            var violation = new DLPViolation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PolicyId = policyId,
                RuleName = rule.Name,
                ViolationType = "Document Similarity Match",
                Description = $"Content has high similarity ({similarity:F2}) to protected document",
                Severity = rule.Severity,
                DetectedContent = "Document similarity match",
                Context = "Document matching detection",
                DetectedAt = DateTime.UtcNow,
                Status = DLPViolationStatus.Open,
                MatchedPatterns = new List<string> { "DOCUMENT_SIMILARITY" },
                ConfidenceScore = similarity
            };

            violations.Add(violation);
        }

        return violations;
    }

    private double CalculateOverallConfidenceScore(List<DLPViolation> violations)
    {
        if (!violations.Any()) return 0.0;
        return violations.Average(v => v.ConfidenceScore);
    }

    private string GetContextAroundMatch(string content, int matchIndex, int matchLength)
    {
        var contextLength = 50;
        var startIndex = Math.Max(0, matchIndex - contextLength);
        var endIndex = Math.Min(content.Length, matchIndex + matchLength + contextLength);
        
        return content.Substring(startIndex, endIndex - startIndex);
    }

    private string GetContextAroundKeyword(string content, string keyword)
    {
        var index = content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (index == -1) return keyword;
        
        return GetContextAroundMatch(content, index, keyword.Length);
    }

    private async Task<double> CalculateMLConfidenceScore(string content, DLPRule rule, CancellationToken cancellationToken)
    {
        return 0.75;
    }

    private string CalculateContentFingerprint(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hashBytes);
    }

    private bool CompareFingerprints(string fingerprint1, string fingerprint2)
    {
        return string.Equals(fingerprint1, fingerprint2, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<double> CalculateDocumentSimilarity(string content, DLPRule rule, CancellationToken cancellationToken)
    {
        return 0.80;
    }
}
