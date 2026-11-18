using FluentValidation;
using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Application.Staff.Validators;

public class GiveEmployeeAdvanceRequestValidator : AbstractValidator<GiveEmployeeAdvanceRequest>
{
    private static readonly string[] AllowedModes = ["Cash", "Bank", "UPI"];
    private static readonly string[] AllowedPlans = ["OneTime", "MonthlyInstalment"];

    public GiveEmployeeAdvanceRequestValidator()
    {
        RuleFor(x => x.StaffId)
            .GreaterThan(0).WithMessage("Please select an employee.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Advance amount must be greater than zero.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Advance date is required.");

        RuleFor(x => x.PaymentMode)
            .NotEmpty().WithMessage("Payment mode is required.")
            .Must(m => AllowedModes.Contains(m, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Payment mode must be Cash, Bank, or UPI.");

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Purpose is required.")
            .MaximumLength(200).WithMessage("Purpose cannot exceed 200 characters.");

        RuleFor(x => x.RecoveryPlan)
            .NotEmpty().WithMessage("Recovery plan is required.")
            .Must(p => AllowedPlans.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Recovery plan must be OneTime or MonthlyInstalment.");

        When(x => string.Equals(x.RecoveryPlan, "MonthlyInstalment", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.MonthlyInstalmentAmount)
                .NotNull().WithMessage("Monthly instalment amount is required.")
                .GreaterThan(0).WithMessage("Monthly instalment amount must be greater than zero.")
                .LessThanOrEqualTo(x => x.Amount).WithMessage("Monthly instalment cannot exceed the advance amount.");
        });

        When(x => x.IsManagerOverride, () =>
        {
            RuleFor(x => x.ManagerOverrideReason)
                .NotEmpty().WithMessage("Manager override reason is required when overriding salary cap.")
                .MinimumLength(5).WithMessage("Override reason must be at least 5 characters.");
        });
    }
}
