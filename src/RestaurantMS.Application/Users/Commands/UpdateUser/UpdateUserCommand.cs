using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Users.Common;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Domain.Enums;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    long Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    long RoleId) : IRequest<Result<UserDto>>, IRequirePermission, IAuditableCommand
{
    public string RequiredPermission => Permissions.Users.Edit;
    public string EntityName => "User";
    public string EntityId => Id.ToString();
    public AuditAction Action => AuditAction.Update;
}
