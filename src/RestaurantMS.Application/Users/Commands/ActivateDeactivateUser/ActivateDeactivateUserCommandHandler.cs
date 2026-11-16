using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Users.Commands.ActivateDeactivateUser;

public class ActivateDeactivateUserCommandHandler : IRequestHandler<ActivateDeactivateUserCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateDeactivateUserCommandHandler(
        IUserRepository userRepository,
        ICurrentUser currentUser,
        IDateTime dateTime,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ActivateDeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure(new Error("User.NotFound", "User not found."));
        }

        if (user.UserId.Equals("admin", StringComparison.OrdinalIgnoreCase) && !request.IsActive)
        {
            return Result.Failure(new Error("User.CannotDeactivateAdmin", "The default administrator account cannot be deactivated."));
        }

        user.IsActive = request.IsActive;
        user.UpdatedAt = _dateTime.UtcNow;
        user.UpdatedBy = _currentUser.UserId ?? "System";

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
