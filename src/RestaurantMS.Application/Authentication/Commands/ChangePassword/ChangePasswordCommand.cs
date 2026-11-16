using MediatR;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Authentication.Commands.ChangePassword;

public record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword) : IRequest<Result>;
