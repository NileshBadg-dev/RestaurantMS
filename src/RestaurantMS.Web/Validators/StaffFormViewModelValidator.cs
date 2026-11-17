using FluentValidation;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Validators;

public class StaffFormViewModelValidator : AbstractValidator<StaffFormViewModel>
{
    public StaffFormViewModelValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^[6-9]\d{9}$").WithMessage("Mobile number must be 10 digits starting with 6, 7, 8, or 9.");

        RuleFor(x => x.MonthlySalary)
            .GreaterThanOrEqualTo(0).WithMessage("Monthly salary cannot be negative.");
    }
}
