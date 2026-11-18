using FluentValidation;
using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Application.Staff.Validators;

public class RecordAdvanceRecoveryRequestValidator : AbstractValidator<RecordAdvanceRecoveryRequest>
{
    private static readonly string[] AllowedModes = ["Cash", "Bank", "UPI", "SalaryDeduction"];

    public RecordAdvanceRecoveryRequestValidator()
    {
        RuleFor(x => x.StaffId)
            .GreaterThan(0).WithMessage("Please select an employee.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Recovery amount must be greater than zero.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Recovery date is required.");

        RuleFor(x => x.PaymentMode)
            .NotEmpty().WithMessage("Payment mode is required.")
            .Must(m => AllowedModes.Contains(m, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Payment mode must be Cash, Bank, UPI, or SalaryDeduction.");
    }
}
