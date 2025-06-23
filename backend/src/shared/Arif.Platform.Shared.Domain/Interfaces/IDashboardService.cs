using Arif.Platform.Shared.Domain.Entities;

namespace Arif.Platform.Shared.Domain.Interfaces;

public interface IDashboardService
{
    Task<List<DashboardWidget>> GetDashboardWidgetsAsync(string dashboardId, CancellationToken cancellationToken = default);
    Task<DashboardConfiguration> GetDashboardConfigurationAsync(string dashboardId, CancellationToken cancellationToken = default);
    Task<string> CreateCustomDashboardAsync(DashboardConfiguration configuration, CancellationToken cancellationToken = default);
    Task UpdateDashboardAsync(string dashboardId, DashboardConfiguration configuration, CancellationToken cancellationToken = default);
    Task DeleteDashboardAsync(string dashboardId, CancellationToken cancellationToken = default);
    Task<List<DashboardMetric>> GetRealTimeMetricsAsync(string dashboardId, CancellationToken cancellationToken = default);
    Task<List<AlertRule>> GetActiveAlertsAsync(string dashboardId, CancellationToken cancellationToken = default);
    Task<AlertRule> CreateAlertRuleAsync(AlertRuleConfiguration configuration, CancellationToken cancellationToken = default);
    Task UpdateAlertRuleAsync(string alertId, AlertRuleConfiguration configuration, CancellationToken cancellationToken = default);
    Task DeleteAlertRuleAsync(string alertId, CancellationToken cancellationToken = default);
}

public interface IIncidentManagementService
{
    Task<List<Incident>> GetActiveIncidentsAsync(CancellationToken cancellationToken = default);
    Task<Incident> CreateIncidentAsync(IncidentCreationRequest request, CancellationToken cancellationToken = default);
    Task UpdateIncidentStatusAsync(string incidentId, IncidentStatus status, string notes, CancellationToken cancellationToken = default);
    Task AssignIncidentAsync(string incidentId, string assigneeId, CancellationToken cancellationToken = default);
    Task<List<IncidentAction>> GetIncidentHistoryAsync(string incidentId, CancellationToken cancellationToken = default);
    Task AddIncidentNoteAsync(string incidentId, string note, string authorId, CancellationToken cancellationToken = default);
    Task EscalateIncidentAsync(string incidentId, EscalationLevel level, CancellationToken cancellationToken = default);
    Task ResolveIncidentAsync(string incidentId, string resolution, CancellationToken cancellationToken = default);
}

public interface IRequestCorrelationService
{
    Task<string> GenerateCorrelationIdAsync(CancellationToken cancellationToken = default);
    Task<RequestTrace> StartRequestTraceAsync(string correlationId, string serviceName, string operationName, CancellationToken cancellationToken = default);
    Task EndRequestTraceAsync(string traceId, bool success, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default);
    Task<List<RequestTrace>> GetRequestTraceAsync(string correlationId, CancellationToken cancellationToken = default);
    Task<List<ServiceCall>> GetServiceCallChainAsync(string correlationId, CancellationToken cancellationToken = default);
    Task LogServiceCallAsync(string correlationId, string fromService, string toService, string operation, TimeSpan duration, bool success, CancellationToken cancellationToken = default);
}
