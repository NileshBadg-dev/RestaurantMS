using FluentValidation;

namespace RestaurantMS.Application.Users.Commands.ActivateDeactivateUser;

public class ActivateDeactivateUserCommandValidator : AbstractValidator<ActivateDeactivateUserCommand>
{
    public ActivateDeactivateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("User Id is required.");
    }
}
