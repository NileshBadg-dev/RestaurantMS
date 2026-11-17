using FluentValidation;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Validators;

public class CustomerFormViewModelValidator : AbstractValidator<CustomerFormViewModel>
{
    public CustomerFormViewModelValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(100).WithMessage("Customer name cannot exceed 100 characters.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^[6-9]\d{9}$").WithMessage("Mobile number must be 10 digits starting with 6, 7, 8, or 9.");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit cannot be negative.");

        RuleFor(x => x.CreditDays)
            .GreaterThanOrEqualTo(0).WithMessage("Credit days cannot be negative.");
    }
}
