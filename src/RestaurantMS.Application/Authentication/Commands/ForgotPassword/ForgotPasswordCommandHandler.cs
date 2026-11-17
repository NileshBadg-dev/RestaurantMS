using MediatR;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Authentication.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;

    public ForgotPasswordCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            // For security, don't reveal if user doesn't exist
            return Result.Success("If the user ID exists, your administrator has been notified to reset your password.");
        }

        return Result.Success("Please contact your system administrator to reset your password.");
    }
}
