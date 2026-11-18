using FluentValidation;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Validation;

namespace RestaurantMS.Application.Common.Validation;

public class StaffProfileValidationRequestValidator : AbstractValidator<StaffProfileValidationRequest>
{
    public StaffProfileValidationRequestValidator(IDateTime dateTime)
    {
        RuleFor(x => x.EmployeeCode)
            .NotEmpty().WithMessage("Employee code is required (e.g. EMP-010).")
            .Must(IndianValidationRules.IsValidShortCode).WithMessage("Employee code must be 2–16 letters, numbers, hyphens or underscores.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Staff full name is required.")
            .Length(2, 120).WithMessage("Staff full name must be between 2 and 120 characters.");

        RuleFor(x => x.Mobile)
            .NotEmpty().WithMessage("Staff mobile number is required.")
            .Must(IndianValidationRules.IsValidMobile).WithMessage("Enter a valid 10-digit mobile number starting with 6, 7, 8, or 9.");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .Must(IndianValidationRules.IsValidEmail).WithMessage("Enter a valid email address.");
        });

        RuleFor(x => x.JoiningDate)
            .Must(d => d.Date <= dateTime.UtcNow.AddHours(5).AddMinutes(30).Date)
            .WithMessage("Joining date cannot be in the future.");

        RuleFor(x => x.BasicSalary)
            .GreaterThan(0m).WithMessage("Basic salary must be greater than Rs 0.")
            .LessThanOrEqualTo(500000m).WithMessage("Basic salary cannot exceed Rs 5,00,000.")
            .Must(s => IndianValidationRules.HasMaxDecimals(s, 2)).WithMessage("Salary can have at most 2 decimal places.");

        When(x => !string.IsNullOrWhiteSpace(x.Pan), () =>
        {
            RuleFor(x => x.Pan)
                .Must(IndianValidationRules.IsValidPan).WithMessage("Enter a valid 10-character PAN (e.g. ABCDE1234F).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.UpiId), () =>
        {
            RuleFor(x => x.UpiId)
                .Must(IndianValidationRules.IsValidUpiId).WithMessage("Enter a valid UPI ID (e.g. staff@okicici).");
        });
    }
}
