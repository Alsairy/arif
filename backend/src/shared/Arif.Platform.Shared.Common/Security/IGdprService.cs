namespace Arif.Platform.Shared.Common.Security;

public interface IGdprService
{
    Task<GdprDataExport> ExportUserDataAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserDataAsync(Guid userId, bool hardDelete = false, CancellationToken cancellationToken = default);
    Task<bool> AnonymizeUserDataAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ConsentRecord> RecordConsentAsync(Guid userId, ConsentType consentType, bool granted, string? details = null, CancellationToken cancellationToken = default);
    Task<bool> WithdrawConsentAsync(Guid userId, ConsentType consentType, CancellationToken cancellationToken = default);
    Task<List<ConsentRecord>> GetUserConsentsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasValidConsentAsync(Guid userId, ConsentType consentType, CancellationToken cancellationToken = default);
    Task<DataProcessingRecord> RecordDataProcessingAsync(Guid userId, DataProcessingType processingType, string purpose, string? details = null, CancellationToken cancellationToken = default);
    Task<List<DataProcessingRecord>> GetDataProcessingHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GdprDataExport
{
    public Guid UserId { get; set; }
    public DateTime ExportDate { get; set; }
    public Dictionary<string, object> PersonalData { get; set; } = new();
    public List<string> DataSources { get; set; } = new();
    public string Format { get; set; } = "JSON";
}

public class ConsentRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public ConsentType ConsentType { get; set; }
    public bool IsGranted { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class DataProcessingRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DataProcessingType ProcessingType { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
    public string LegalBasis { get; set; } = string.Empty;
}

public enum ConsentType
{
    DataProcessing,
    Marketing,
    Analytics,
    Cookies,
    ThirdPartySharing,
    ProfileAnalysis,
    AutomatedDecisionMaking
}

public enum DataProcessingType
{
    Collection,
    Storage,
    Processing,
    Analysis,
    Sharing,
    Transfer,
    Deletion,
    Anonymization
}
