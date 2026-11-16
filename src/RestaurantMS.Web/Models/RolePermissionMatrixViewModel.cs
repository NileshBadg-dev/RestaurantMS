using RestaurantMS.Application.Roles.Common;

namespace RestaurantMS.Web.Models;

public class RolePermissionMatrixViewModel
{
    public RolePermissionMatrixDto Matrix { get; set; } = new();
    public List<long> SelectedPermissionIds { get; set; } = [];
}
