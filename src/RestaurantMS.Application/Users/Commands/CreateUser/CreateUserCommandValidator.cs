using FluentValidation;
using RestaurantMS.Application.Common.Repositories;

namespace RestaurantMS.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .Length(4, 20).WithMessage("User ID must be between 4 and 20 characters.")
            .Matches(@"^[a-zA-Z0-9]+$").WithMessage("User ID must contain only letters and digits.")
            .MustAsync(async (userId, ct) => await userRepository.IsUserIdUniqueAsync(userId, null, ct))
            .WithMessage("User ID already exists.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("A valid email address is required.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^[6-9]\d{9}$").WithMessage("Mobile number must be 10 digits starting with 6-9.");
        });

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Role is required.");
    }
}
