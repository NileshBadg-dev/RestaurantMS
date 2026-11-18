using FluentValidation;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Inventory;

namespace RestaurantMS.Application.Inventory.Validators;

public class StockConsumptionRequestValidator : AbstractValidator<StockConsumptionRequest>
{
    public StockConsumptionRequestValidator(IDateTime dateTime)
    {
        RuleFor(x => x.StockItemId)
            .GreaterThan(0).WithMessage("Select a stock item.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0m).WithMessage("Consumption quantity must be greater than zero.");

        RuleFor(x => x.ConsumptionDate)
            .Must(d => d.Date <= dateTime.UtcNow.AddHours(5).AddMinutes(30).Date)
            .WithMessage("Consumption date cannot be in the future.");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Kitchen / department is required.");
    }
}
