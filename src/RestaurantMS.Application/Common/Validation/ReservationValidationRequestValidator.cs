using FluentValidation;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Validation;

namespace RestaurantMS.Application.Common.Validation;

public class ReservationValidationRequestValidator : AbstractValidator<ReservationValidationRequest>
{
    public ReservationValidationRequestValidator(IDateTime dateTime)
    {
        RuleFor(x => x.GuestName)
            .NotEmpty().WithMessage("Guest name is required for reservation.")
            .Length(2, 120).WithMessage("Guest name must be between 2 and 120 characters.");

        RuleFor(x => x.Mobile)
            .NotEmpty().WithMessage("Guest mobile number is required.")
            .Must(IndianValidationRules.IsValidMobile).WithMessage("Enter a valid 10-digit mobile number starting with 6, 7, 8, or 9.");

        RuleFor(x => x.GuestCount)
            .InclusiveBetween(1, 50).WithMessage("Party size must be between 1 and 50 guests.");

        RuleFor(x => x.ReservationDate)
            .Must(d => d.Date >= dateTime.UtcNow.AddHours(5).AddMinutes(30).Date)
            .WithMessage("Reservation date cannot be in the past.");

        RuleFor(x => x.TimeSlot)
            .NotEmpty().WithMessage("Select a dining time slot.");
    }
}
