using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.Users.Common;
using RestaurantMS.Domain.Entities;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasherService passwordHasher,
        ICurrentUser currentUser,
        IDateTime dateTime,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure<UserDto>(new Error("User.InvalidRole", "Selected role does not exist."));
        }

        var user = new AppUser
        {
            UserId = request.UserId.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            RoleId = request.RoleId,
            IsActive = true,
            MustChangePassword = false,
            CreatedAt = _dateTime.UtcNow,
            CreatedBy = _currentUser.UserId ?? "System"
        };

        _userRepository.Add(user);
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
