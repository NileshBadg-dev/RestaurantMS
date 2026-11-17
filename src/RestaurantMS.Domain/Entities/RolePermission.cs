using RestaurantMS.SharedKernel;

namespace RestaurantMS.Domain.Entities;

public class RolePermission : BaseEntity
{
    public long RoleId { get; set; }
    public Role? Role { get; set; }

    public long PermissionId { get; set; }
    public Permission? Permission { get; set; }
}
