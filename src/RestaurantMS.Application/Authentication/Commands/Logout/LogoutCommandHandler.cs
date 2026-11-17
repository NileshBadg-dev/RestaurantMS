using MediatR;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Authentication.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    public Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }
}
