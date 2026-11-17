using RestaurantMS.SharedKernel;

namespace RestaurantMS.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }

    public ICollection<AppUser> Users { get; set; } = [];
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
