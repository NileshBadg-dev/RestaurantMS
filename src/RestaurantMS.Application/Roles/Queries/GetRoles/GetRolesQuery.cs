using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Roles.Common;
using RestaurantMS.Domain.Constants;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Roles.Queries.GetRoles;

public record GetRolesQuery : IRequest<Result<IReadOnlyList<RoleDto>>>, IRequirePermission
{
    public string RequiredPermission => Permissions.Roles.View;
}
