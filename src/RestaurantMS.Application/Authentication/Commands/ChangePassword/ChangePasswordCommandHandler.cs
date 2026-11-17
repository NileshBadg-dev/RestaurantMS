using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Authentication.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IDateTime _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasher,
        IDateTime dateTime,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(new Error("Auth.UserNotFound", "User not found."));
        }

        if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.CurrentPassword))
        {
            return Result.Failure(new Error("Auth.InvalidCurrentPassword", "Current password is incorrect."));
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.MustChangePassword = false;
        user.UpdatedAt = _dateTime.UtcNow;
        user.UpdatedBy = request.UserId;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
