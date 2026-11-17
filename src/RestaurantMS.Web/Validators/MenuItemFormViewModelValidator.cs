using FluentValidation;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Validators;

public class MenuItemFormViewModelValidator : AbstractValidator<MenuItemFormViewModel>
{
    public MenuItemFormViewModelValidator()
    {
        RuleFor(x => x.ItemCode)
            .NotEmpty().WithMessage("Item code is required.")
            .MaximumLength(20).WithMessage("Item code cannot exceed 20 characters.");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("Item name (English) is required.")
            .MaximumLength(100).WithMessage("Item name cannot exceed 100 characters.");

        RuleFor(x => x.NameMr)
            .NotEmpty().WithMessage("Item name (Marathi) is required.")
            .MaximumLength(100).WithMessage("Item name cannot exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative.");
    }
}
