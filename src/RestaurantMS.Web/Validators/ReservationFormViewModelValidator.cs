using FluentValidation;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Validators;

public class ReservationFormViewModelValidator : AbstractValidator<ReservationFormViewModel>
{
    public ReservationFormViewModelValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(100).WithMessage("Customer name cannot exceed 100 characters.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^[6-9]\d{9}$").WithMessage("Mobile number must be 10 digits starting with 6, 7, 8, or 9.");

        RuleFor(x => x.GuestCount)
            .GreaterThan(0).WithMessage("Guest count must be at least 1.");

        RuleFor(x => x.TableNumber)
            .NotEmpty().WithMessage("Table number is required.");
    }
}
