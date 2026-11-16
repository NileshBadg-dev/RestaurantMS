using FluentValidation;

namespace RestaurantMS.Application.FeatureToggles.Commands.SaveFeatureToggles;

public class SaveFeatureTogglesCommandValidator : AbstractValidator<SaveFeatureTogglesCommand>
{
    public SaveFeatureTogglesCommandValidator()
    {
        RuleFor(x => x.Toggles)
            .NotNull().WithMessage("Toggles dictionary cannot be null.");
    }
}
