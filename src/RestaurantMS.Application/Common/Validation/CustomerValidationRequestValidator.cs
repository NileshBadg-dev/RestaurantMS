using FluentValidation;
using RestaurantMS.Application.Common.Models.Validation;

namespace RestaurantMS.Application.Common.Validation;

public class CustomerValidationRequestValidator : AbstractValidator<CustomerValidationRequest>
{
    public CustomerValidationRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Customer full name is required.")
            .Length(2, 120).WithMessage("Customer full name must be between 2 and 120 characters.");

        RuleFor(x => x.Mobile)
            .NotEmpty().WithMessage("Customer mobile number is required.")
            .Must(IndianValidationRules.IsValidMobile).WithMessage("Enter a valid 10-digit mobile number starting with 6, 7, 8, or 9.");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .Must(IndianValidationRules.IsValidEmail).WithMessage("Enter a valid email address (e.g. name@example.com).");
        });

        When(x => x.CreditAllowed, () =>
        {
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Customer address is required when Udhari credit is enabled.")
                .MinimumLength(5).WithMessage("Enter a complete address (at least 5 characters) for Udhari credit.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Gstin), () =>
        {
            RuleFor(x => x.Gstin)
                .Must(IndianValidationRules.IsValidGstin).WithMessage("Enter a valid 15-character GSTIN with correct checksum (e.g. 27AAAAA0000A1Z5).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Pan), () =>
        {
            RuleFor(x => x.Pan)
                .Must(IndianValidationRules.IsValidPan).WithMessage("Enter a valid 10-character PAN (e.g. ABCDE1234F).");
        });

        RuleFor(x => x.CreditLimit)
            .InclusiveBetween(0m, 1000000m).WithMessage("Credit limit must be between Rs 0 and Rs 10,00,000.")
            .Must(v => IndianValidationRules.HasMaxDecimals(v, 2)).WithMessage("Credit limit can have at most 2 decimal places.");
    }
}
