using FluentValidation;
using RestaurantMS.Application.Common.Models.Accounting;

namespace RestaurantMS.Application.Common.Validation;

public class PostVoucherRequestValidator : AbstractValidator<PostVoucherRequest>
{
    public PostVoucherRequestValidator()
    {
        RuleFor(x => x.VoucherType)
            .NotEmpty().WithMessage("Voucher type is required (Payment, Receipt, Journal, Contra, Sales, Purchase).");

        RuleFor(x => x.Narration)
            .NotEmpty().WithMessage("Voucher narration is required for audit compliance.")
            .MaximumLength(500).WithMessage("Narration must not exceed 500 characters.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("A voucher must have at least 2 double-entry ledger lines.")
            .Must(l => l is { Count: >= 2 }).WithMessage("A voucher must have at least 2 double-entry ledger lines.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.AccountCode)
                .NotEmpty().WithMessage("Ledger account code is required on every line.");

            line.RuleFor(l => l.Debit)
                .GreaterThanOrEqualTo(0m).WithMessage("Debit amount cannot be negative.");

            line.RuleFor(l => l.Credit)
                .GreaterThanOrEqualTo(0m).WithMessage("Credit amount cannot be negative.");

            line.RuleFor(l => l)
                .Must(l => (l.Debit > 0m && l.Credit == 0m) || (l.Credit > 0m && l.Debit == 0m))
                .WithMessage("Each ledger line must have either a positive Debit or a positive Credit (not both).");
        });

        RuleFor(x => x.Lines)
            .Must(lines => lines is not null && Math.Abs(lines.Sum(l => l.Debit) - lines.Sum(l => l.Credit)) < 0.01m)
            .WithMessage("Total Debit must equal Total Credit before posting the voucher.");
    }
}
