using FluentValidation;
using RestaurantMS.Application.Common.Models.Validation;

namespace RestaurantMS.Application.Common.Validation;

public class MenuItemValidationRequestValidator : AbstractValidator<MenuItemValidationRequest>
{
    public MenuItemValidationRequestValidator()
    {
        RuleFor(x => x.ShortCode)
            .NotEmpty().WithMessage("Short code is required (e.g. M01).")
            .Must(IndianValidationRules.IsValidShortCode).WithMessage("Short code must be 2–16 letters, numbers, hyphens or underscores.");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("Dish name (English) is required.")
            .Length(2, 120).WithMessage("Dish name (English) must be between 2 and 120 characters.");

        RuleFor(x => x.NameMr)
            .NotEmpty().WithMessage("Dish name (Marathi) is required.")
            .MaximumLength(120).WithMessage("Dish name (Marathi) must not exceed 120 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Select a menu category for this dish.");

        RuleFor(x => x.Price)
            .GreaterThan(0m).WithMessage("Price must be greater than Rs 0.00.")
            .LessThanOrEqualTo(100000m).WithMessage("Price cannot exceed Rs 1,00,000.00.")
            .Must(p => IndianValidationRules.HasMaxDecimals(p, 2)).WithMessage("Price can have at most 2 decimal places.");

        RuleFor(x => x.GstPercent)
            .InclusiveBetween(0m, 28m).WithMessage("GST percentage must be between 0% and 28%.");

        RuleFor(x => x.PrepTimeMinutes)
            .InclusiveBetween(1, 240).WithMessage("Preparation time must be between 1 and 240 minutes.");
    }
}
