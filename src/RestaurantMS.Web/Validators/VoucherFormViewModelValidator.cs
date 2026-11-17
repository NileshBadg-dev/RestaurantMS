using FluentValidation;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Validators;

public class VoucherFormViewModelValidator : AbstractValidator<VoucherFormViewModel>
{
    public VoucherFormViewModelValidator()
    {
        RuleFor(x => x.VoucherType)
            .NotEmpty().WithMessage("Voucher type is required.");

        RuleFor(x => x.Lines)
            .Must(lines => lines != null && lines.Count >= 2)
            .WithMessage("At least two journal lines are required.");

        RuleFor(x => x)
            .Must(x => x.TotalDebit == x.TotalCredit && x.TotalDebit > 0)
            .WithMessage("Total debits must equal total credits and be greater than zero.");
    }
}
