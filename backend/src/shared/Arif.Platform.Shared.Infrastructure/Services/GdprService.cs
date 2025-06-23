using Microsoft.Extensions.Logging;
using System.Text.Json;
using Arif.Platform.Shared.Infrastructure.Data;
using Arif.Platform.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Arif.Platform.Shared.Common.Security;

public class GdprService : IGdprService
{
    private readonly ArifPlatformDbContext _dbContext;
    private readonly ILogger<GdprService> _logger;
    private readonly IAuditLogger _auditLogger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GdprService(
        ArifPlatformDbContext dbContext,
        ILogger<GdprService> logger,
        IAuditLogger auditLogger,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _auditLogger = auditLogger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GdprDataExport> ExportUserDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _dbContext.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found");

            var export = new GdprDataExport
            {
                UserId = userId,
                ExportDate = DateTime.UtcNow
            };

            export.PersonalData["User"] = new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Username,
                user.Language,
                user.TimeZone,
                user.CreatedAt,
                user.LastLoginAt,
                TenantName = user.Tenant?.Name
            };
            export.DataSources.Add("Users");

            var chatSessions = await _dbContext.ChatSessions
                .Where(cs => cs.UserIdentifier == userId.ToString())
                .Include(cs => cs.Messages)
                .ToListAsync(cancellationToken);

            if (chatSessions.Any())
            {
                export.PersonalData["ChatSessions"] = chatSessions.Select(cs => new
                {
                    cs.Id,
                    cs.CreatedAt,
                    cs.Status,
                    cs.Channel,
                    Messages = cs.Messages.Select(m => new
                    {
                        m.Id,
                        m.Content,
                        m.MessageType,
                        m.CreatedAt,
                        IsFromUser = !m.IsFromBot
                    })
                });
                export.DataSources.Add("ChatSessions");
            }

            var auditLogs = await _dbContext.AuditLogs
                .Where(al => al.UserId == userId)
                .OrderByDescending(al => al.Timestamp)
                .Take(1000)
                .ToListAsync(cancellationToken);

            if (auditLogs.Any())
            {
                export.PersonalData["AuditLogs"] = auditLogs.Select(al => new
                {
                    al.Timestamp,
                    al.EventType,
                    al.Action,
                    al.Description,
                    al.IpAddress
                });
                export.DataSources.Add("AuditLogs");
            }

            await _auditLogger.LogDataAccessAsync("UserData", DataAccessType.Export, userId, user.TenantId, 
                new { ExportedSources = export.DataSources }, cancellationToken);

            await RecordDataProcessingAsync(userId, DataProcessingType.Processing, "GDPR Data Export", 
                "User requested data export under GDPR Article 15", cancellationToken);

            _logger.LogInformation("GDPR data export completed for user {UserId}", userId);
            return export;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export GDPR data for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteUserDataAsync(Guid userId, bool hardDelete = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                return false;

            if (hardDelete)
            {
                var chatSessions = await _dbContext.ChatSessions
                    .Where(cs => cs.UserIdentifier == userId.ToString())
                    .Include(cs => cs.Messages)
                    .ToListAsync(cancellationToken);

                foreach (var session in chatSessions)
                {
                    _dbContext.ChatMessages.RemoveRange(session.Messages);
                }
                _dbContext.ChatSessions.RemoveRange(chatSessions);

                var auditLogs = await _dbContext.AuditLogs
                    .Where(al => al.UserId == userId)
                    .ToListAsync(cancellationToken);
                _dbContext.AuditLogs.RemoveRange(auditLogs);

                _dbContext.Users.Remove(user);
            }
            else
            {
                await AnonymizeUserDataAsync(userId, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _auditLogger.LogSecurityEventAsync(SecurityEventType.ConfigurationChange, 
                $"User data {(hardDelete ? "deleted" : "anonymized")} under GDPR", 
                userId, user.TenantId, new { HardDelete = hardDelete }, cancellationToken);

            _logger.LogInformation("User data {Action} for user {UserId}", hardDelete ? "deleted" : "anonymized", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user data for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> AnonymizeUserDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                return false;

            var anonymizedId = Guid.NewGuid();
            user.Email = $"anonymized-{anonymizedId}@deleted.local";
            user.FirstName = "Anonymized";
            user.LastName = "User";
            user.Username = $"anonymized-{anonymizedId}";
            user.IsActive = false;
            user.PasswordHash = string.Empty;
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiresAt = null;

            var chatSessions = await _dbContext.ChatSessions
                .Where(cs => cs.UserIdentifier == userId.ToString())
                .Include(cs => cs.Messages)
                .ToListAsync(cancellationToken);

            foreach (var session in chatSessions)
            {
                foreach (var message in session.Messages.Where(m => !m.IsFromBot))
                {
                    message.Content = "[Message content anonymized]";
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await RecordDataProcessingAsync(userId, DataProcessingType.Anonymization, "GDPR Right to be Forgotten", 
                "User data anonymized under GDPR Article 17", cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to anonymize user data for user {UserId}", userId);
            return false;
        }
    }

    public async Task<ConsentRecord> RecordConsentAsync(Guid userId, ConsentType consentType, bool granted, string? details = null, CancellationToken cancellationToken = default)
    {
        var consent = new ConsentRecord
        {
            UserId = userId,
            ConsentType = consentType,
            IsGranted = granted,
            Details = details,
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent()
        };

        var consentEntity = new GdprConsent
        {
            Id = consent.Id,
            UserId = consent.UserId,
            ConsentType = consent.ConsentType.ToString(),
            IsGranted = consent.IsGranted,
            Timestamp = consent.Timestamp,
            ExpiresAt = consent.ExpiresAt,
            Details = consent.Details,
            IpAddress = consent.IpAddress,
            UserAgent = consent.UserAgent
        };

        _dbContext.GdprConsents.Add(consentEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogUserActionAsync($"ConsentRecorded_{consentType}", userId, Guid.Empty, 
            new { ConsentType = consentType, Granted = granted }, cancellationToken);

        return consent;
    }

    public async Task<bool> WithdrawConsentAsync(Guid userId, ConsentType consentType, CancellationToken cancellationToken = default)
    {
        var existingConsent = await _dbContext.GdprConsents
            .Where(c => c.UserId == userId && c.ConsentType == consentType.ToString())
            .OrderByDescending(c => c.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingConsent == null || !existingConsent.IsGranted)
            return false;

        await RecordConsentAsync(userId, consentType, false, "Consent withdrawn by user", cancellationToken);
        return true;
    }

    public async Task<List<ConsentRecord>> GetUserConsentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var consents = await _dbContext.GdprConsents
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.Timestamp)
            .ToListAsync(cancellationToken);

        return consents.Select(c => new ConsentRecord
        {
            Id = c.Id,
            UserId = c.UserId,
            ConsentType = Enum.Parse<ConsentType>(c.ConsentType),
            IsGranted = c.IsGranted,
            Timestamp = c.Timestamp,
            ExpiresAt = c.ExpiresAt,
            Details = c.Details,
            IpAddress = c.IpAddress,
            UserAgent = c.UserAgent
        }).ToList();
    }

    public async Task<bool> HasValidConsentAsync(Guid userId, ConsentType consentType, CancellationToken cancellationToken = default)
    {
        var latestConsent = await _dbContext.GdprConsents
            .Where(c => c.UserId == userId && c.ConsentType == consentType.ToString())
            .OrderByDescending(c => c.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestConsent == null)
            return false;

        if (!latestConsent.IsGranted)
            return false;

        if (latestConsent.ExpiresAt.HasValue && latestConsent.ExpiresAt.Value < DateTime.UtcNow)
            return false;

        return true;
    }

    public async Task<DataProcessingRecord> RecordDataProcessingAsync(Guid userId, DataProcessingType processingType, string purpose, string? details = null, CancellationToken cancellationToken = default)
    {
        var record = new DataProcessingRecord
        {
            UserId = userId,
            ProcessingType = processingType,
            Purpose = purpose,
            Details = details,
            LegalBasis = GetLegalBasis(processingType, purpose)
        };

        var entity = new GdprDataProcessing
        {
            Id = record.Id,
            UserId = record.UserId,
            ProcessingType = record.ProcessingType.ToString(),
            Purpose = record.Purpose,
            Timestamp = record.Timestamp,
            Details = record.Details,
            LegalBasis = record.LegalBasis
        };

        _dbContext.GdprDataProcessing.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return record;
    }

    public async Task<List<DataProcessingRecord>> GetDataProcessingHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.GdprDataProcessing
            .Where(dp => dp.UserId == userId)
            .OrderByDescending(dp => dp.Timestamp)
            .ToListAsync(cancellationToken);

        return records.Select(r => new DataProcessingRecord
        {
            Id = r.Id,
            UserId = r.UserId,
            ProcessingType = Enum.Parse<DataProcessingType>(r.ProcessingType),
            Purpose = r.Purpose,
            Timestamp = r.Timestamp,
            Details = r.Details,
            LegalBasis = r.LegalBasis
        }).ToList();
    }

    private string? GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        var xForwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
            return xForwardedFor.Split(',')[0].Trim();

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Request.Headers["User-Agent"].FirstOrDefault();
    }

    private static string GetLegalBasis(DataProcessingType processingType, string purpose)
    {
        return processingType switch
        {
            DataProcessingType.Collection => "Legitimate interest - Service provision",
            DataProcessingType.Processing => "Contract performance",
            DataProcessingType.Analysis => "Legitimate interest - Service improvement",
            DataProcessingType.Sharing => "Consent",
            DataProcessingType.Transfer => "Consent",
            DataProcessingType.Deletion => "Legal obligation - GDPR compliance",
            DataProcessingType.Anonymization => "Legal obligation - GDPR compliance",
            _ => "Legitimate interest"
        };
    }
}
