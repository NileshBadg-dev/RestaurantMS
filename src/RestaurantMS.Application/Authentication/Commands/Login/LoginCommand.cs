using MediatR;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Authentication.Commands.Login;

public record LoginCommand(string UserId, string Password, bool RememberMe = false) : IRequest<Result<LoginResultDto>>;
