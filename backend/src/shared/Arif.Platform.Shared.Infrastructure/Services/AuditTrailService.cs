using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Arif.Platform.Shared.Domain.Interfaces;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Common.Security;
using System.Text.Json;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class AuditTrailService : IAuditTrailService
{
    private readonly ILogger<AuditTrailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;
    private readonly HttpClient _httpClient;

    public AuditTrailService(
        ILogger<AuditTrailService> logger,
        IConfiguration configuration,
        IAuditLogger auditLogger,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogger = auditLogger;
        _httpClient = httpClient;
    }

    public async Task<List<AuditTrailEntry>> GetAuditTrailAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving audit trail for tenant {TenantId} from {StartDate} to {EndDate}", tenantId, startDate, endDate);

            var auditEntries = new List<AuditTrailEntry>();

            return auditEntries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit trail for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<AuditTrailEntry> CreateAuditEntryAsync(AuditTrailEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating audit entry for tenant {TenantId}, action {Action}", entry.TenantId, entry.Action);

            entry.Id = Guid.NewGuid();
            entry.Timestamp = DateTime.UtcNow;
            entry.RequestId = Guid.NewGuid().ToString();

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "Audit entry created", entry.UserId, entry.TenantId, new { EntryId = entry.Id, Action = entry.Action });

            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit entry for tenant {TenantId}", entry.TenantId);
            throw;
        }
    }

    public async Task<List<AuditTrailEntry>> SearchAuditTrailAsync(Guid tenantId, AuditSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching audit trail for tenant {TenantId} with criteria", tenantId);

            var auditEntries = new List<AuditTrailEntry>();

            var filteredEntries = await ApplySearchFilters(auditEntries, criteria, cancellationToken);
            var sortedEntries = ApplySorting(filteredEntries, criteria);
            var paginatedEntries = ApplyPagination(sortedEntries, criteria);

            return paginatedEntries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching audit trail for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<AuditTrailReport> GenerateAuditReportAsync(Guid tenantId, DateTime startDate, DateTime endDate, AuditReportType reportType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating audit report for tenant {TenantId}, type {ReportType} from {StartDate} to {EndDate}", tenantId, reportType, startDate, endDate);

            var report = new AuditTrailReport
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ReportName = $"{reportType} Audit Report",
                ReportType = reportType,
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = Guid.Empty,
                Format = AuditReportFormat.JSON,
                Status = AuditReportStatus.Generating
            };

            var auditEntries = await GetAuditTrailAsync(tenantId, startDate, endDate, cancellationToken);
            report.Entries = auditEntries;
            report.Summary = GenerateReportSummary(auditEntries);

            var reportData = new
            {
                Summary = report.Summary,
                Entries = auditEntries.Take(1000).ToList(),
                GeneratedAt = report.GeneratedAt,
                ReportType = reportType.ToString(),
                DateRange = new { StartDate = startDate, EndDate = endDate }
            };

            report.ReportData = JsonSerializer.Serialize(reportData);
            report.Status = AuditReportStatus.Completed;

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "Audit report generated", null, tenantId, new { ReportId = report.Id, ReportType = reportType });

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating audit report for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> ArchiveAuditTrailAsync(Guid tenantId, DateTime beforeDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Archiving audit trail for tenant {TenantId} before {BeforeDate}", tenantId, beforeDate);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "Audit trail archived", null, tenantId, new { BeforeDate = beforeDate });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving audit trail for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<AuditTrailMetrics> GetAuditMetricsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditEntries = await GetAuditTrailAsync(tenantId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, cancellationToken);

            var metrics = new AuditTrailMetrics
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MetricsDate = DateTime.UtcNow,
                TotalEvents = auditEntries.Count,
                SecurityEvents = auditEntries.Count(e => e.EventType == AuditEventType.SecurityEvent),
                DataAccessEvents = auditEntries.Count(e => e.EventType == AuditEventType.DataAccess),
                ConfigurationEvents = auditEntries.Count(e => e.EventType == AuditEventType.ConfigurationChange),
                FailedEvents = auditEntries.Count(e => e.Result == AuditResult.Failure),
                UniqueUsers = auditEntries.Where(e => e.UserId.HasValue).Select(e => e.UserId.Value).Distinct().Count(),
                UniqueSessions = auditEntries.Select(e => e.SessionId).Distinct().Count()
            };

            metrics.EventsByHour = CalculateEventsByHour(auditEntries);
            metrics.EventsByDay = CalculateEventsByDay(auditEntries);
            metrics.TopUsers = CalculateTopUsers(auditEntries);
            metrics.TopResources = CalculateTopResources(auditEntries);
            metrics.Anomalies = await DetectAnomalies(auditEntries, cancellationToken);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit metrics for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<List<AuditTrailEntry>> GetUserActivityAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving user activity for user {UserId} from {StartDate} to {EndDate}", userId, startDate, endDate);

            var auditEntries = new List<AuditTrailEntry>();

            return auditEntries.Where(e => e.UserId == userId).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<AuditTrailEntry>> GetSystemEventsAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving system events for tenant {TenantId} from {StartDate} to {EndDate}", tenantId, startDate, endDate);

            var auditEntries = await GetAuditTrailAsync(tenantId, startDate, endDate, cancellationToken);

            return auditEntries.Where(e => e.EventType == AuditEventType.SystemEvent).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system events for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> ValidateAuditIntegrityAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating audit integrity for tenant {TenantId}", tenantId);

            var auditEntries = await GetAuditTrailAsync(tenantId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, cancellationToken);

            var integrityValid = await PerformIntegrityChecks(auditEntries, cancellationToken);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "Audit integrity validation completed", null, tenantId, new { IntegrityValid = integrityValid, EntriesChecked = auditEntries.Count });

            return integrityValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating audit integrity for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<AuditTrailExport> ExportAuditTrailAsync(Guid tenantId, DateTime startDate, DateTime endDate, ExportFormat format, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Exporting audit trail for tenant {TenantId} in format {Format} from {StartDate} to {EndDate}", tenantId, format, startDate, endDate);

            var export = new AuditTrailExport
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ExportName = $"AuditTrail_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}",
                Format = format,
                StartDate = startDate,
                EndDate = endDate,
                RequestedAt = DateTime.UtcNow,
                RequestedBy = Guid.Empty,
                Status = ExportStatus.Processing
            };

            var auditEntries = await GetAuditTrailAsync(tenantId, startDate, endDate, cancellationToken);

            var exportData = await GenerateExportData(auditEntries, format, cancellationToken);
            var filePath = await SaveExportFile(export, exportData, cancellationToken);

            export.FilePath = filePath;
            export.FileSize = new FileInfo(filePath).Length;
            export.CompletedAt = DateTime.UtcNow;
            export.Status = ExportStatus.Completed;
            export.ExpiresAt = DateTime.UtcNow.AddDays(7);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, "Audit trail exported", null, tenantId, new { ExportId = export.Id, Format = format, EntriesExported = auditEntries.Count });

            return export;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit trail for tenant {TenantId}", tenantId);
            throw;
        }
    }

    private async Task<List<AuditTrailEntry>> ApplySearchFilters(List<AuditTrailEntry> entries, AuditSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var filteredEntries = entries.AsQueryable();

        if (criteria.UserId.HasValue)
            filteredEntries = filteredEntries.Where(e => e.UserId == criteria.UserId.Value);

        if (!string.IsNullOrEmpty(criteria.UserName))
            filteredEntries = filteredEntries.Where(e => e.UserName.Contains(criteria.UserName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(criteria.Action))
            filteredEntries = filteredEntries.Where(e => e.Action.Contains(criteria.Action, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(criteria.Resource))
            filteredEntries = filteredEntries.Where(e => e.Resource.Contains(criteria.Resource, StringComparison.OrdinalIgnoreCase));

        if (criteria.EventType.HasValue)
            filteredEntries = filteredEntries.Where(e => e.EventType == criteria.EventType.Value);

        if (criteria.StartDate.HasValue)
            filteredEntries = filteredEntries.Where(e => e.Timestamp >= criteria.StartDate.Value);

        if (criteria.EndDate.HasValue)
            filteredEntries = filteredEntries.Where(e => e.Timestamp <= criteria.EndDate.Value);

        if (!string.IsNullOrEmpty(criteria.IpAddress))
            filteredEntries = filteredEntries.Where(e => e.IpAddress == criteria.IpAddress);

        if (criteria.Result.HasValue)
            filteredEntries = filteredEntries.Where(e => e.Result == criteria.Result.Value);

        if (!string.IsNullOrEmpty(criteria.SessionId))
            filteredEntries = filteredEntries.Where(e => e.SessionId == criteria.SessionId);

        if (!string.IsNullOrEmpty(criteria.RequestId))
            filteredEntries = filteredEntries.Where(e => e.RequestId == criteria.RequestId);

        return filteredEntries.ToList();
    }

    private List<AuditTrailEntry> ApplySorting(List<AuditTrailEntry> entries, AuditSearchCriteria criteria)
    {
        return criteria.SortBy.ToLower() switch
        {
            "timestamp" => criteria.SortDirection == SortDirection.Ascending 
                ? entries.OrderBy(e => e.Timestamp).ToList()
                : entries.OrderByDescending(e => e.Timestamp).ToList(),
            "action" => criteria.SortDirection == SortDirection.Ascending
                ? entries.OrderBy(e => e.Action).ToList()
                : entries.OrderByDescending(e => e.Action).ToList(),
            "username" => criteria.SortDirection == SortDirection.Ascending
                ? entries.OrderBy(e => e.UserName).ToList()
                : entries.OrderByDescending(e => e.UserName).ToList(),
            _ => entries.OrderByDescending(e => e.Timestamp).ToList()
        };
    }

    private List<AuditTrailEntry> ApplyPagination(List<AuditTrailEntry> entries, AuditSearchCriteria criteria)
    {
        var skip = (criteria.PageNumber - 1) * criteria.PageSize;
        return entries.Skip(skip).Take(criteria.PageSize).ToList();
    }

    private AuditReportSummary GenerateReportSummary(List<AuditTrailEntry> entries)
    {
        return new AuditReportSummary
        {
            TotalEntries = entries.Count,
            SuccessfulActions = entries.Count(e => e.Result == AuditResult.Success),
            FailedActions = entries.Count(e => e.Result == AuditResult.Failure),
            SecurityEvents = entries.Count(e => e.EventType == AuditEventType.SecurityEvent),
            DataAccessEvents = entries.Count(e => e.EventType == AuditEventType.DataAccess),
            ConfigurationChanges = entries.Count(e => e.EventType == AuditEventType.ConfigurationChange),
            EventsByType = entries.GroupBy(e => e.EventType.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            EventsByUser = entries.Where(e => !string.IsNullOrEmpty(e.UserName)).GroupBy(e => e.UserName).ToDictionary(g => g.Key, g => g.Count()),
            EventsByResource = entries.GroupBy(e => e.Resource).ToDictionary(g => g.Key, g => g.Count()),
            Trends = GenerateTrends(entries)
        };
    }

    private List<AuditTrend> GenerateTrends(List<AuditTrailEntry> entries)
    {
        return entries
            .GroupBy(e => new { Date = e.Timestamp.Date, EventType = e.EventType })
            .Select(g => new AuditTrend
            {
                Date = g.Key.Date,
                EventCount = g.Count(),
                EventType = g.Key.EventType,
                TrendDirection = 0.0
            })
            .OrderBy(t => t.Date)
            .ToList();
    }

    private Dictionary<string, int> CalculateEventsByHour(List<AuditTrailEntry> entries)
    {
        return entries
            .GroupBy(e => e.Timestamp.Hour)
            .ToDictionary(g => g.Key.ToString("D2"), g => g.Count());
    }

    private Dictionary<string, int> CalculateEventsByDay(List<AuditTrailEntry> entries)
    {
        return entries
            .GroupBy(e => e.Timestamp.Date)
            .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count());
    }

    private Dictionary<string, int> CalculateTopUsers(List<AuditTrailEntry> entries)
    {
        return entries
            .Where(e => !string.IsNullOrEmpty(e.UserName))
            .GroupBy(e => e.UserName)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<string, int> CalculateTopResources(List<AuditTrailEntry> entries)
    {
        return entries
            .GroupBy(e => e.Resource)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private async Task<List<AuditAnomaly>> DetectAnomalies(List<AuditTrailEntry> entries, CancellationToken cancellationToken)
    {
        var anomalies = new List<AuditAnomaly>();

        var failureRate = entries.Count > 0 ? (double)entries.Count(e => e.Result == AuditResult.Failure) / entries.Count : 0;
        if (failureRate > 0.1)
        {
            anomalies.Add(new AuditAnomaly
            {
                AnomalyType = "High Failure Rate",
                Description = $"Failure rate of {failureRate:P} exceeds normal threshold",
                DetectedAt = DateTime.UtcNow,
                Severity = AnomalySeverity.High,
                AnomalyData = new Dictionary<string, object> { ["FailureRate"] = failureRate },
                IsResolved = false
            });
        }

        var uniqueIpCount = entries.Select(e => e.IpAddress).Distinct().Count();
        if (uniqueIpCount > 100)
        {
            anomalies.Add(new AuditAnomaly
            {
                AnomalyType = "Unusual IP Activity",
                Description = $"Detected {uniqueIpCount} unique IP addresses, which is unusually high",
                DetectedAt = DateTime.UtcNow,
                Severity = AnomalySeverity.Medium,
                AnomalyData = new Dictionary<string, object> { ["UniqueIpCount"] = uniqueIpCount },
                IsResolved = false
            });
        }

        return anomalies;
    }

    private async Task<bool> PerformIntegrityChecks(List<AuditTrailEntry> entries, CancellationToken cancellationToken)
    {
        var integrityChecks = new List<bool>();

        integrityChecks.Add(CheckTimestampSequence(entries));
        integrityChecks.Add(CheckRequiredFields(entries));
        integrityChecks.Add(await CheckDataConsistency(entries, cancellationToken));

        return integrityChecks.All(check => check);
    }

    private bool CheckTimestampSequence(List<AuditTrailEntry> entries)
    {
        var sortedEntries = entries.OrderBy(e => e.Timestamp).ToList();
        
        for (int i = 1; i < sortedEntries.Count; i++)
        {
            if (sortedEntries[i].Timestamp < sortedEntries[i - 1].Timestamp)
            {
                return false;
            }
        }

        return true;
    }

    private bool CheckRequiredFields(List<AuditTrailEntry> entries)
    {
        return entries.All(e => 
            e.Id != Guid.Empty &&
            e.TenantId != Guid.Empty &&
            !string.IsNullOrEmpty(e.Action) &&
            !string.IsNullOrEmpty(e.Resource) &&
            e.Timestamp != default);
    }

    private async Task<bool> CheckDataConsistency(List<AuditTrailEntry> entries, CancellationToken cancellationToken)
    {
        return true;
    }

    private async Task<string> GenerateExportData(List<AuditTrailEntry> entries, ExportFormat format, CancellationToken cancellationToken)
    {
        return format switch
        {
            ExportFormat.JSON => JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true }),
            ExportFormat.CSV => GenerateCsvData(entries),
            ExportFormat.XML => GenerateXmlData(entries),
            _ => JsonSerializer.Serialize(entries)
        };
    }

    private string GenerateCsvData(List<AuditTrailEntry> entries)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Id,TenantId,UserId,UserName,Action,Resource,EventType,Timestamp,IpAddress,Result");

        foreach (var entry in entries)
        {
            csv.AppendLine($"{entry.Id},{entry.TenantId},{entry.UserId},{entry.UserName},{entry.Action},{entry.Resource},{entry.EventType},{entry.Timestamp:yyyy-MM-dd HH:mm:ss},{entry.IpAddress},{entry.Result}");
        }

        return csv.ToString();
    }

    private string GenerateXmlData(List<AuditTrailEntry> entries)
    {
        var xml = new System.Text.StringBuilder();
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine("<AuditTrail>");

        foreach (var entry in entries)
        {
            xml.AppendLine("  <Entry>");
            xml.AppendLine($"    <Id>{entry.Id}</Id>");
            xml.AppendLine($"    <TenantId>{entry.TenantId}</TenantId>");
            xml.AppendLine($"    <UserId>{entry.UserId}</UserId>");
            xml.AppendLine($"    <UserName>{entry.UserName}</UserName>");
            xml.AppendLine($"    <Action>{entry.Action}</Action>");
            xml.AppendLine($"    <Resource>{entry.Resource}</Resource>");
            xml.AppendLine($"    <EventType>{entry.EventType}</EventType>");
            xml.AppendLine($"    <Timestamp>{entry.Timestamp:yyyy-MM-dd HH:mm:ss}</Timestamp>");
            xml.AppendLine($"    <IpAddress>{entry.IpAddress}</IpAddress>");
            xml.AppendLine($"    <Result>{entry.Result}</Result>");
            xml.AppendLine("  </Entry>");
        }

        xml.AppendLine("</AuditTrail>");
        return xml.ToString();
    }

    private async Task<string> SaveExportFile(AuditTrailExport export, string data, CancellationToken cancellationToken)
    {
        var exportDirectory = Path.Combine(Path.GetTempPath(), "audit-exports");
        Directory.CreateDirectory(exportDirectory);

        var fileName = $"{export.ExportName}.{export.Format.ToString().ToLower()}";
        var filePath = Path.Combine(exportDirectory, fileName);

        await File.WriteAllTextAsync(filePath, data, cancellationToken);

        return filePath;
    }
}
