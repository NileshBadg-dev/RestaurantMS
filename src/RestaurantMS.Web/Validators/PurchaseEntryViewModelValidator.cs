using FluentValidation;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Validators;

public class PurchaseEntryViewModelValidator : AbstractValidator<PurchaseEntryViewModel>
{
    public PurchaseEntryViewModelValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier is required.");

        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("Invoice number is required.")
            .MaximumLength(50).WithMessage("Invoice number cannot exceed 50 characters.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one purchase item is required.");

        RuleFor(x => x.GrandTotal)
            .GreaterThan(0).WithMessage("Grand total must be greater than zero.");
    }
}
