using MediatR;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Authentication.Commands.Logout;

public record LogoutCommand(string UserId) : IRequest<Result>;
