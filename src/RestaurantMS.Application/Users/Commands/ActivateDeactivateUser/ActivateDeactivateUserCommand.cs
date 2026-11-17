using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Domain.Enums;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Users.Commands.ActivateDeactivateUser;

public record ActivateDeactivateUserCommand(long Id, bool IsActive) : IRequest<Result>, IRequirePermission, IAuditableCommand
{
    public string RequiredPermission => Permissions.Users.Edit;
    public string EntityName => "User";
    public string EntityId => Id.ToString();
    public AuditAction Action => IsActive ? AuditAction.Activate : AuditAction.Deactivate;
}
