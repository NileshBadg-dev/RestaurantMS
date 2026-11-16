using MediatR;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Authentication.Commands.ForgotPassword;

public record ForgotPasswordCommand(string UserId) : IRequest<Result<string>>;
