using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Domain.Enums;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Roles.Commands.SaveRolePermissionMatrix;

public record SaveRolePermissionMatrixCommand(long RoleId, List<long> PermissionIds)
    : IRequest<Result>, IRequirePermission, IAuditableCommand
{
    public string RequiredPermission => Permissions.Roles.Edit;
    public string EntityName => "RolePermissions";
    public string EntityId => RoleId.ToString();
    public AuditAction Action => AuditAction.Update;
}
