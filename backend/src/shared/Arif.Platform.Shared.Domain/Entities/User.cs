using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class User : BaseEntity, ITenantAware
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(10)]
    public string Language { get; set; } = "ar";
    
    [MaxLength(10)]
    public string PreferredLanguage { get; set; } = "ar";
    
    [MaxLength(50)]
    public string TimeZone { get; set; } = "Asia/Riyadh";
    
    public bool IsActive { get; set; } = true;
    
    public bool EmailConfirmed { get; set; } = false;
    
    public DateTime? LastLoginAt { get; set; }
    
    [MaxLength(255)]
    public string? PasswordResetToken { get; set; }
    
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
    
    [MaxLength(255)]
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiresAt { get; set; }
    
    public Guid TenantId { get; set; }
    
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
