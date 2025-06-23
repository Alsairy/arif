using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Arif.Platform.Shared.Domain.Entities;
using Arif.Platform.Shared.Domain.Interfaces;
using Arif.Platform.Shared.Common.Security;

namespace Arif.Platform.Shared.Infrastructure.Services;

public class IncidentManagementService : IIncidentManagementService
{
    private readonly ILogger<IncidentManagementService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogger _auditLogger;
    private readonly Dictionary<string, Incident> _incidents;

    public IncidentManagementService(
        ILogger<IncidentManagementService> logger,
        IConfiguration configuration,
        IAuditLogger auditLogger)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogger = auditLogger;
        _incidents = new Dictionary<string, Incident>();
    }

    public async Task<List<Incident>> GetActiveIncidentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeIncidents = _incidents.Values
                .Where(incident => incident.Status == IncidentStatus.Open || incident.Status == IncidentStatus.InProgress)
                .OrderByDescending(incident => incident.CreatedAt)
                .ToList();

            await _auditLogger.LogUserActionAsync(
                "active_incidents_retrieved",
                Guid.Empty, // TODO: Get current user ID
                Guid.Empty, // TODO: Get current tenant ID
                $"Retrieved {activeIncidents.Count} active incidents"
            );

            return activeIncidents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active incidents");
            throw;
        }
    }

    public async Task<Incident> CreateIncidentAsync(IncidentCreationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var incident = new Incident
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Severity = request.Severity,
                Status = IncidentStatus.Open,
                ReportedBy = request.ReportedBy,
                AffectedServices = request.AffectedServices,
                EscalationLevel = EscalationLevel.None,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Actions = new List<IncidentAction>()
            };

            var creationAction = new IncidentAction
            {
                Id = Guid.NewGuid(),
                IncidentId = incident.Id.ToString(),
                ActionType = IncidentActionType.Created,
                Description = $"Incident created: {incident.Title}",
                PerformedBy = request.ReportedBy,
                CreatedAt = DateTime.UtcNow
            };

            incident.Actions.Add(creationAction);
            _incidents[incident.Id.ToString()] = incident;

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                $"Incident created: {incident.Title} - ID: {incident.Id} - Severity: {incident.Severity}"
            );

            _logger.LogWarning("New incident created: {IncidentTitle} - ID: {IncidentId} - Severity: {Severity}", 
                incident.Title, incident.Id, incident.Severity);

            return incident;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incident: {IncidentTitle}", request.Title);
            throw;
        }
    }

    public async Task UpdateIncidentStatusAsync(string incidentId, IncidentStatus status, string notes, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_incidents.TryGetValue(incidentId, out var incident))
            {
                throw new KeyNotFoundException($"Incident not found: {incidentId}");
            }

            var previousStatus = incident.Status;
            incident.Status = status;
            incident.UpdatedAt = DateTime.UtcNow;

            if (status == IncidentStatus.Resolved)
            {
                incident.ResolvedAt = DateTime.UtcNow;
            }

            var statusAction = new IncidentAction
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                ActionType = IncidentActionType.StatusChanged,
                Description = $"Status changed from {previousStatus} to {status}. Notes: {notes}",
                PerformedBy = "system", // TODO: Get current user
                CreatedAt = DateTime.UtcNow
            };

            incident.Actions.Add(statusAction);

            await _auditLogger.LogUserActionAsync(
                "incident_status_updated",
                Guid.Empty, // TODO: Get current user ID
                Guid.Empty, // TODO: Get current tenant ID
                $"Incident {incidentId} status changed from {previousStatus} to {status}"
            );

            _logger.LogInformation("Incident status updated: {IncidentId} - {PreviousStatus} -> {NewStatus}", 
                incidentId, previousStatus, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating incident status: {IncidentId}", incidentId);
            throw;
        }
    }

    public async Task AssignIncidentAsync(string incidentId, string assigneeId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_incidents.TryGetValue(incidentId, out var incident))
            {
                throw new KeyNotFoundException($"Incident not found: {incidentId}");
            }

            var previousAssignee = incident.AssignedTo;
            incident.AssignedTo = assigneeId;
            incident.UpdatedAt = DateTime.UtcNow;

            var assignmentAction = new IncidentAction
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                ActionType = IncidentActionType.Assigned,
                Description = $"Incident assigned to {assigneeId}",
                PerformedBy = "system", // TODO: Get current user
                CreatedAt = DateTime.UtcNow
            };

            incident.Actions.Add(assignmentAction);

            await _auditLogger.LogUserActionAsync(
                "incident_assigned",
                Guid.Empty, // TODO: Get current user ID
                Guid.Empty, // TODO: Get current tenant ID
                $"Incident {incidentId} assigned to {assigneeId}"
            );

            _logger.LogInformation("Incident assigned: {IncidentId} -> {AssigneeId}", incidentId, assigneeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning incident: {IncidentId}", incidentId);
            throw;
        }
    }

    public async Task<List<IncidentAction>> GetIncidentHistoryAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_incidents.TryGetValue(incidentId, out var incident))
            {
                throw new KeyNotFoundException($"Incident not found: {incidentId}");
            }

            var history = incident.Actions
                .OrderByDescending(action => action.CreatedAt)
                .ToList();

            await Task.CompletedTask;
            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident history: {IncidentId}", incidentId);
            throw;
        }
    }

    public async Task AddIncidentNoteAsync(string incidentId, string note, string authorId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_incidents.TryGetValue(incidentId, out var incident))
            {
                throw new KeyNotFoundException($"Incident not found: {incidentId}");
            }

            var noteAction = new IncidentAction
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                ActionType = IncidentActionType.NoteAdded,
                Description = note,
                PerformedBy = authorId,
                CreatedAt = DateTime.UtcNow
            };

            incident.Actions.Add(noteAction);
            incident.UpdatedAt = DateTime.UtcNow;

            await _auditLogger.LogUserActionAsync(
                "incident_note_added",
                Guid.Empty, // TODO: Get current user ID
                Guid.Empty, // TODO: Get current tenant ID
                $"Note added to incident {incidentId} by {authorId}"
            );

            _logger.LogInformation("Note added to incident: {IncidentId} by {AuthorId}", incidentId, authorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding note to incident: {IncidentId}", incidentId);
            throw;
        }
    }

    public async Task EscalateIncidentAsync(string incidentId, EscalationLevel level, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_incidents.TryGetValue(incidentId, out var incident))
            {
                throw new KeyNotFoundException($"Incident not found: {incidentId}");
            }

            var previousLevel = incident.EscalationLevel;
            incident.EscalationLevel = level;
            incident.UpdatedAt = DateTime.UtcNow;

            var escalationAction = new IncidentAction
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                ActionType = IncidentActionType.Escalated,
                Description = $"Incident escalated from {previousLevel} to {level}",
                PerformedBy = "system", // TODO: Get current user
                CreatedAt = DateTime.UtcNow
            };

            incident.Actions.Add(escalationAction);

            await _auditLogger.LogSecurityEventAsync(
                SecurityEventType.ConfigurationChange,
                $"Incident {incidentId} escalated to {level}"
            );

            _logger.LogWarning("Incident escalated: {IncidentId} - {PreviousLevel} -> {NewLevel}", 
                incidentId, previousLevel, level);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating incident: {IncidentId}", incidentId);
            throw;
        }
    }

    public async Task ResolveIncidentAsync(string incidentId, string resolution, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_incidents.TryGetValue(incidentId, out var incident))
            {
                throw new KeyNotFoundException($"Incident not found: {incidentId}");
            }

            incident.Status = IncidentStatus.Resolved;
            incident.Resolution = resolution;
            incident.ResolvedAt = DateTime.UtcNow;
            incident.UpdatedAt = DateTime.UtcNow;

            var resolutionAction = new IncidentAction
            {
                Id = Guid.NewGuid(),
                IncidentId = incidentId,
                ActionType = IncidentActionType.Resolved,
                Description = $"Incident resolved: {resolution}",
                PerformedBy = "system", // TODO: Get current user
                CreatedAt = DateTime.UtcNow
            };

            incident.Actions.Add(resolutionAction);

            await _auditLogger.LogUserActionAsync(
                "incident_resolved",
                Guid.Empty, // TODO: Get current user ID
                Guid.Empty, // TODO: Get current tenant ID
                $"Incident {incidentId} resolved: {resolution}"
            );

            _logger.LogInformation("Incident resolved: {IncidentId} - {Resolution}", incidentId, resolution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving incident: {IncidentId}", incidentId);
            throw;
        }
    }
}
