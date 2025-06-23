using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class ChatSession : BaseEntity, ITenantAware
{
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty;
    
    public Guid ChatbotId { get; set; }
    
    [MaxLength(255)]
    public string? UserIdentifier { get; set; }
    
    [MaxLength(50)]
    public string Channel { get; set; } = "web";
    
    [MaxLength(20)]
    public string Status { get; set; } = "active";
    
    public DateTime? EndedAt { get; set; }
    
    public string Metadata { get; set; } = "{}";
    
    public Guid TenantId { get; set; }
    
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Chatbot Chatbot { get; set; } = null!;
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
