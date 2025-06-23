using System.ComponentModel.DataAnnotations;

namespace Arif.Platform.Shared.Domain.Entities;

public class Permission : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    
    public bool IsSystemPermission { get; set; } = false;
    
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
