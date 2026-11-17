using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Users.Commands.ResetUserPassword;

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public ResetUserPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasher,
        ICurrentUser currentUser,
        IDateTime dateTime,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure(new Error("User.NotFound", "User not found."));
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.MustChangePassword = true;
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = _dateTime.UtcNow;
        user.UpdatedBy = _currentUser.UserId ?? "System";

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
