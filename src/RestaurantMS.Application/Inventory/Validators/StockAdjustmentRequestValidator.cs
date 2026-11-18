using FluentValidation;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Inventory;

namespace RestaurantMS.Application.Inventory.Validators;

public class StockAdjustmentRequestValidator : AbstractValidator<StockAdjustmentRequest>
{
    public StockAdjustmentRequestValidator(IDateTime dateTime)
    {
        RuleFor(x => x.StockItemId)
            .GreaterThan(0).WithMessage("Select a stock item.");

        RuleFor(x => x.AdjustmentDate)
            .Must(d => d.Date <= dateTime.UtcNow.AddHours(5).AddMinutes(30).Date)
            .WithMessage("Adjustment date cannot be in the future.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Adjustment reason is required.")
            .MaximumLength(300).WithMessage("Reason must not exceed 300 characters.");
    }
}
