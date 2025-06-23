using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class Tenant : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Subdomain { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string ContactEmail { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? LogoUrl { get; set; }
    
    [MaxLength(10)]
    public string Language { get; set; } = "ar";
    
    [MaxLength(10)]
    public string DefaultLanguage { get; set; } = "ar";
    
    [MaxLength(50)]
    public string TimeZone { get; set; } = "Asia/Riyadh";
    
    public bool IsActive { get; set; } = true;
    
    public int MaxUsers { get; set; } = 10;
    
    public DateTime? SubscriptionExpiresAt { get; set; }
    
    [MaxLength(50)]
    public string SubscriptionPlan { get; set; } = "free";
    
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Chatbot> Chatbots { get; set; } = new List<Chatbot>();
}
