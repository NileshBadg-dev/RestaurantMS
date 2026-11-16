using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Roles.Common;
using RestaurantMS.Application.Users.Common;

namespace RestaurantMS.Web.Models;

public class UserListViewModel
{
    public PagedResult<UserDto> Users { get; set; } = new();
    public string? SearchTerm { get; set; }
    public IReadOnlyList<RoleDto> Roles { get; set; } = [];
}
