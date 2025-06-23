using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class Chatbot : BaseEntity, ITenantAware
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(10)]
    public string Language { get; set; } = "ar";
    
    public bool IsActive { get; set; } = true;
    
    public string Configuration { get; set; } = "{}";
    
    public string KnowledgeBase { get; set; } = "{}";
    
    [MaxLength(255)]
    public string? AvatarUrl { get; set; }
    
    [MaxLength(100)]
    public string WelcomeMessage { get; set; } = "مرحباً! كيف يمكنني مساعدتك؟";
    
    public Guid TenantId { get; set; }
    
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
}
