using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class Role : BaseEntity, ITenantAware
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Description { get; set; }
    
    public bool IsSystemRole { get; set; } = false;
    
    public Guid TenantId { get; set; }
    
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
