using FluentValidation;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Inventory;

namespace RestaurantMS.Application.Inventory.Validators;

public class StockWastageRequestValidator : AbstractValidator<StockWastageRequest>
{
    public StockWastageRequestValidator(IDateTime dateTime)
    {
        RuleFor(x => x.StockItemId)
            .GreaterThan(0).WithMessage("Select a stock item.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0m).WithMessage("Wastage quantity must be greater than zero.");

        RuleFor(x => x.WastageDate)
            .Must(d => d.Date <= dateTime.UtcNow.AddHours(5).AddMinutes(30).Date)
            .WithMessage("Wastage date cannot be in the future.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Wastage reason is required.")
            .MaximumLength(300).WithMessage("Wastage reason must not exceed 300 characters.");
    }
}
