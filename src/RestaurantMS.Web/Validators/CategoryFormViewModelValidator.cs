using FluentValidation;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Validators;

public class CategoryFormViewModelValidator : AbstractValidator<CategoryFormViewModel>
{
    public CategoryFormViewModelValidator()
    {
        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("Category name (English) is required.")
            .MaximumLength(50).WithMessage("Category name cannot exceed 50 characters.");

        RuleFor(x => x.NameMr)
            .NotEmpty().WithMessage("Category name (Marathi) is required.")
            .MaximumLength(50).WithMessage("Category name cannot exceed 50 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(1).WithMessage("Display order must be at least 1.");
    }
}
