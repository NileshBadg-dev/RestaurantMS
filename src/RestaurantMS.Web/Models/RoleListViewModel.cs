using RestaurantMS.Application.Roles.Common;

namespace RestaurantMS.Web.Models;

public class RoleListViewModel
{
    public IReadOnlyList<RoleDto> Roles { get; set; } = [];
}
