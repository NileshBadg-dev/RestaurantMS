using FluentValidation;
using RestaurantMS.Application.Common.Models.Billing;

namespace RestaurantMS.Application.Common.Validation;

public class SettleBillRequestValidator : AbstractValidator<SettleBillRequest>
{
    public SettleBillRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Add at least one item to settle the bill.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemName)
                .NotEmpty().WithMessage("Bill item name is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Item quantity must be at least 1.");

            item.RuleFor(i => i.Rate)
                .GreaterThanOrEqualTo(0m).WithMessage("Item rate cannot be negative.");
        });

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0m).WithMessage("Discount amount cannot be negative.");

        RuleFor(x => x.PackingCharge)
            .GreaterThanOrEqualTo(0m).WithMessage("Packing charge cannot be negative.");

        RuleFor(x => x.GstPercent)
            .InclusiveBetween(0m, 28m).WithMessage("GST rate must be between 0% and 28%.");

        When(x => x.IsManagerOverride, () =>
        {
            RuleFor(x => x.ManagerOverrideReason)
                .NotEmpty().WithMessage("Enter a manager override reason when exceeding credit limit.");
        });
    }
}
