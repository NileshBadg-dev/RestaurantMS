using FluentValidation;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Inventory;

namespace RestaurantMS.Application.Inventory.Validators;

public class RecordPurchaseRequestValidator : AbstractValidator<RecordPurchaseRequest>
{
    public RecordPurchaseRequestValidator(IDateTime dateTime)
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier is required.");

        RuleFor(x => x.SupplierName)
            .NotEmpty().WithMessage("Supplier is required.");

        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("Invoice number is required.")
            .MaximumLength(60).WithMessage("Invoice number must not exceed 60 characters.");

        RuleFor(x => x.InvoiceDate)
            .Must(d => d.Date <= dateTime.UtcNow.AddHours(5).AddMinutes(30).Date)
            .WithMessage("Invoice date cannot be in the future.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Add at least one item line to record a purchase.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemName)
                .NotEmpty().WithMessage("Item name is required.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0m).WithMessage("Quantity must be greater than zero.");

            line.RuleFor(l => l.Rate)
                .GreaterThan(0m).WithMessage("Rate must be greater than zero.");

            line.RuleFor(l => l.TaxPercent)
                .InclusiveBetween(0m, 28m).WithMessage("Tax rate must be between 0% and 28%.");
        });
    }
}
