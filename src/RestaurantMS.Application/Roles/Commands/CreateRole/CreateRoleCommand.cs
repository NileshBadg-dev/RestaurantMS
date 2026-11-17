using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Roles.Common;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Domain.Enums;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Roles.Commands.CreateRole;

public record CreateRoleCommand(string Name, string? Description = null) : IRequest<Result<RoleDto>>, IRequirePermission, IAuditableCommand
{
    public string RequiredPermission => Permissions.Roles.Create;
    public string EntityName => "Role";
    public string EntityId => Name;
    public AuditAction Action => AuditAction.Create;
}
