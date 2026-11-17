using RestaurantMS.SharedKernel;

namespace RestaurantMS.Domain.Entities;

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Screen { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
