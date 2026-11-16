using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Domain.Enums;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Users.Commands.ResetUserPassword;

public record ResetUserPasswordCommand(long Id, string NewPassword) : IRequest<Result>, IRequirePermission, IAuditableCommand
{
    public string RequiredPermission => Permissions.Users.Edit;
    public string EntityName => "User";
    public string EntityId => Id.ToString();
    public AuditAction Action => AuditAction.ResetPassword;
}
