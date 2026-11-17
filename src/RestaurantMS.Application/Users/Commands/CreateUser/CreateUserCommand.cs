using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Users.Common;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Domain.Enums;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Users.Commands.CreateUser;

public record CreateUserCommand(
    string UserId,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string Password,
    long RoleId) : IRequest<Result<UserDto>>, IRequirePermission, IAuditableCommand
{
    public string RequiredPermission => Permissions.Users.Create;
    public string EntityName => "User";
    public string EntityId => UserId;
    public AuditAction Action => AuditAction.Create;
}
