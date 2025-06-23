namespace Arif.Platform.Shared.Domain.Entities;

public class GdprDataProcessing : BaseEntity
{
    public Guid UserId { get; set; }
    public string ProcessingType { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
    public string LegalBasis { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}
