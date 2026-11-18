using FluentValidation;
using RestaurantMS.Application.Common.Models.Validation;

namespace RestaurantMS.Application.Common.Validation;

public class SupplierValidationRequestValidator : AbstractValidator<SupplierValidationRequest>
{
    public SupplierValidationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier business name is required.")
            .Length(2, 150).WithMessage("Supplier name must be between 2 and 150 characters.");

        RuleFor(x => x.ContactPerson)
            .NotEmpty().WithMessage("Supplier contact person name is required.");

        RuleFor(x => x.Mobile)
            .NotEmpty().WithMessage("Supplier mobile number is required.")
            .Must(IndianValidationRules.IsValidMobile).WithMessage("Enter a valid 10-digit mobile number starting with 6, 7, 8, or 9.");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .Must(IndianValidationRules.IsValidEmail).WithMessage("Enter a valid email address (e.g. accounts@vendor.com).");
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

        When(x => !string.IsNullOrWhiteSpace(x.UpiId), () =>
        {
            RuleFor(x => x.UpiId)
                .Must(IndianValidationRules.IsValidUpiId).WithMessage("Enter a valid UPI ID (e.g. supplier@okhdfc).");
        });
    }
}
