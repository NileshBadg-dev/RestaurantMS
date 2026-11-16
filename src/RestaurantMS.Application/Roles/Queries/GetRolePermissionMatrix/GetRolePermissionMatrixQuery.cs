using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Roles.Common;
using RestaurantMS.Domain.Constants;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Roles.Queries.GetRolePermissionMatrix;

public record GetRolePermissionMatrixQuery(long RoleId) : IRequest<Result<RolePermissionMatrixDto>>, IRequirePermission
{
    public string RequiredPermission => Permissions.Roles.View;
}
