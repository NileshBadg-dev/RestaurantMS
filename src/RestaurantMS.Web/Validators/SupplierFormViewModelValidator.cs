using FluentValidation;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Validators;

public class SupplierFormViewModelValidator : AbstractValidator<SupplierFormViewModel>
{
    public SupplierFormViewModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(100).WithMessage("Supplier name cannot exceed 100 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^[6-9]\d{9}$").WithMessage("Phone number must be 10 digits starting with 6, 7, 8, or 9.");
    }
}
