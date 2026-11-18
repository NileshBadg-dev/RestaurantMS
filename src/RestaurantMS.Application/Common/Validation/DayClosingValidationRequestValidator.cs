using FluentValidation;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Validation;

namespace RestaurantMS.Application.Common.Validation;

public class DayClosingValidationRequestValidator : AbstractValidator<DayClosingValidationRequest>
{
    public DayClosingValidationRequestValidator(IDateTime dateTime)
    {
        RuleFor(x => x.ClosingDate)
            .Must(d => d.Date <= dateTime.UtcNow.AddHours(5).AddMinutes(30).Date)
            .WithMessage("Day closing date cannot be in the future.");

        RuleFor(x => x.OpeningFloat)
            .GreaterThanOrEqualTo(0m).WithMessage("Opening float cannot be negative.");

        RuleFor(x => x.CountedCash)
            .GreaterThanOrEqualTo(0m).WithMessage("Counted cash in drawer cannot be negative.")
            .Must(v => IndianValidationRules.HasMaxDecimals(v, 2)).WithMessage("Cash amount can have at most 2 decimal places.");

        When(x => Math.Abs(x.CountedCash - x.ExpectedCash) >= 1m, () =>
        {
            RuleFor(x => x.VarianceRemarks)
                .NotEmpty().WithMessage("Cash variance detected. Enter remarks explaining the shortage or excess before closing.");
        });
    }
}
