using FluentValidation;
using RestaurantMS.Application.Common.Models.Validation;

namespace RestaurantMS.Application.Common.Validation;

public class TableMoveValidationRequestValidator : AbstractValidator<TableMoveValidationRequest>
{
    public TableMoveValidationRequestValidator()
    {
        RuleFor(x => x.SourceTableCode)
            .NotEmpty().WithMessage("Select the current source table.");

        RuleFor(x => x.TargetTableCode)
            .NotEmpty().WithMessage("Select a target table to move the order to.")
            .NotEqual(x => x.SourceTableCode, StringComparer.OrdinalIgnoreCase)
            .WithMessage("Target table must be different from the current table.");

        RuleFor(x => x.TargetTableOccupied)
            .Equal(false)
            .WithMessage(x => $"Table {x.TargetTableCode} is already occupied. Choose a free table or merge bills explicitly.");

        When(x => x.ActiveKotCount > 0, () =>
        {
            RuleFor(x => x.ConfirmKotTransfer)
                .Equal(true)
                .WithMessage(x => $"Table {x.SourceTableCode} has {x.ActiveKotCount} active KOT(s) in kitchen. Confirm KOT transfer to {x.TargetTableCode} before moving.");
        });
    }
}
