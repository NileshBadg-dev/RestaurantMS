using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.Users.Common;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICurrentUser currentUser,
        IDateTime dateTime,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(new Error("User.NotFound", "User not found."));
        }

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure<UserDto>(new Error("User.InvalidRole", "Selected role does not exist."));
        }

        user.FullName = request.FullName.Trim();
        user.Email = request.Email?.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.RoleId = request.RoleId;
        user.UpdatedAt = _dateTime.UtcNow;
        user.UpdatedBy = _currentUser.UserId ?? "System";

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new UserDto
        {
            Id = user.Id,
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RoleId = user.RoleId,
            RoleName = role.Name,
            IsActive = user.IsActive,
            MustChangePassword = user.MustChangePassword,
            CreatedAt = user.CreatedAt
        };

        return Result.Success(dto);
    }
}
