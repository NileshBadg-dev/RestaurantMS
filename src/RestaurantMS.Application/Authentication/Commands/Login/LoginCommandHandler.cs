using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IDateTime _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasherService passwordHasher,
        IDateTime dateTime,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<LoginResultDto>(new Error("Auth.InvalidCredentials", "Incorrect User ID or password."));
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > _dateTime.UtcNow)
        {
            var remainingMinutes = (int)Math.Ceiling((user.LockoutEnd.Value - _dateTime.UtcNow).TotalMinutes);
            return Result.Failure<LoginResultDto>(new Error(
                "Auth.LockedOut",
                $"Account is locked due to multiple failed login attempts. Please try again after {remainingMinutes} minute(s)."));
        }

        if (!user.IsActive)
        {
            return Result.Failure<LoginResultDto>(new Error("Auth.InactiveUser", "User account is inactive. Please contact your administrator."));
        }

        var passwordValid = _passwordHasher.VerifyPassword(user.PasswordHash, request.Password);
        if (!passwordValid)
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = _dateTime.UtcNow.AddMinutes(10);
            }

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (user.AccessFailedCount >= 5)
            {
                return Result.Failure<LoginResultDto>(new Error(
                    "Auth.LockedOut",
                    "Account is locked due to multiple failed login attempts. Please try again after 10 minutes."));
            }

            return Result.Failure<LoginResultDto>(new Error("Auth.InvalidCredentials", "Incorrect User ID or password."));
        }

        // Reset failed attempts on success
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var role = user.Role ?? await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);
        var roleWithPerms = role is not null ? await _roleRepository.GetWithPermissionsAsync(role.Id, cancellationToken) : null;
        var permissions = roleWithPerms?.RolePermissions
            .Where(rp => rp.Permission is not null)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToList() ?? [];

        var result = new LoginResultDto
        {
            Success = true,
            IsLockedOut = false,
            MustChangePassword = user.MustChangePassword,
            UserId = user.UserId,
            FullName = user.FullName,
            RoleName = role?.Name ?? string.Empty,
            Permissions = permissions
        };

        return Result.Success(result);
    }
}
