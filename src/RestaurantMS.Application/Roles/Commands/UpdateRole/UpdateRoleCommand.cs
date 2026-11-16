using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Roles.Common;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Domain.Enums;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Roles.Commands.UpdateRole;

public record UpdateRoleCommand(long Id, string Name, string? Description = null) : IRequest<Result<RoleDto>>, IRequirePermission, IAuditableCommand
{
    public string RequiredPermission => Permissions.Roles.Edit;
    public string EntityName => "Role";
    public string EntityId => Id.ToString();
    public AuditAction Action => AuditAction.Update;
}
