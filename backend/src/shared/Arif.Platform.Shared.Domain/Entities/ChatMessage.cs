using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class ChatMessage : BaseEntity, ITenantAware
{
    public Guid SessionId { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string MessageType { get; set; } = "text";
    
    [MaxLength(20)]
    public string Sender { get; set; } = "user";
    
    public bool IsFromBot { get; set; } = false;
    
    public string? Metadata { get; set; }
    
    public Guid TenantId { get; set; }
    
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ChatSession Session { get; set; } = null!;
}
