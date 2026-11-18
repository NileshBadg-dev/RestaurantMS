using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Accounting;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Infrastructure.Services;

public class InMemoryAccountingService : IAccountingService
{
    private readonly IDateTime _dateTime;
    private readonly object _sync = new();
    private readonly List<PostedVoucherDto> _vouchers = new();
    private DateTime? _lockDate = new DateTime(2026, 9, 30);
    private long _nextVoucherId = 200;

    public InMemoryAccountingService(IDateTime dateTime)
    {
        _dateTime = dateTime;
    }

    public Task<bool> IsPeriodLockedAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            var isLocked = _lockDate.HasValue && date.Date <= _lockDate.Value.Date;
            return Task.FromResult(isLocked);
        }
    }

    public Task<DateTime?> GetLockDateAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            return Task.FromResult(_lockDate);
        }
    }

    public Task SetLockDateAsync(DateTime? lockDate, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            _lockDate = lockDate?.Date;
            return Task.CompletedTask;
        }
    }

    public Task<Result<PostedVoucherDto>> PostVoucherAsync(PostVoucherRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Task.FromResult(Result.Failure<PostedVoucherDto>(new Error("Accounting.InvalidRequest", "Voucher request is required.")));
        }

        if (request.Lines is null || request.Lines.Count < 2)
        {
            return Task.FromResult(Result.Failure<PostedVoucherDto>(new Error("Accounting.InvalidLines", "A voucher must have at least two lines (debit and credit).")));
        }

        if (request.Lines.Any(l => l.Debit < 0m || l.Credit < 0m))
        {
            return Task.FromResult(Result.Failure<PostedVoucherDto>(new Error("Accounting.NegativeAmount", "Voucher line amounts cannot be negative.")));
        }

        var totalDebit = Math.Round(request.Lines.Sum(l => l.Debit), 2, MidpointRounding.AwayFromZero);
        var totalCredit = Math.Round(request.Lines.Sum(l => l.Credit), 2, MidpointRounding.AwayFromZero);

        if (totalDebit <= 0m)
        {
            return Task.FromResult(Result.Failure<PostedVoucherDto>(new Error("Accounting.ZeroAmount", "Voucher total must be greater than zero.")));
        }

        if (totalDebit != totalCredit)
        {
            return Task.FromResult(Result.Failure<PostedVoucherDto>(new Error(
                "Accounting.UnbalancedVoucher",
                $"Voucher does not balance: Debit (₹{totalDebit:N2}) must equal Credit (₹{totalCredit:N2}).")));
        }

        lock (_sync)
        {
            if (_lockDate.HasValue && request.VoucherDate.Date <= _lockDate.Value.Date)
            {
                return Task.FromResult(Result.Failure<PostedVoucherDto>(new Error(
                    "Accounting.PeriodLocked",
                    $"Cannot post voucher dated {request.VoucherDate:dd MMM yyyy}: accounting period is locked up to {_lockDate.Value:dd MMM yyyy}.")));
            }

            var id = _nextVoucherId++;
            var prefix = request.VoucherType.ToUpperInvariant() switch
            {
                "PAYMENT" => "PMT",
                "RECEIPT" => "RCP",
                "JOURNAL" => "JRN",
                "CONTRA" => "CNT",
                _ => "VCH"
            };

            var voucherNo = $"{prefix}-{_dateTime.UtcNow:yyyy}-{id:D4}";
            var posted = new PostedVoucherDto(
                id,
                voucherNo,
                request.VoucherDate,
                request.VoucherType,
                request.Narration,
                request.ReferenceNumber,
                totalDebit,
                totalCredit,
                request.Lines.ToList());

            _vouchers.Insert(0, posted);
            return Task.FromResult(Result.Success(posted));
        }
    }

    public Task<IReadOnlyList<PostedVoucherDto>> GetPostedVouchersAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? voucherType = null,
        CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            IEnumerable<PostedVoucherDto> query = _vouchers;

            if (fromDate.HasValue)
            {
                query = query.Where(v => v.VoucherDate.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(v => v.VoucherDate.Date <= toDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(voucherType) && !voucherType.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(v => v.VoucherType.Equals(voucherType, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult<IReadOnlyList<PostedVoucherDto>>(query.ToList());
        }
    }
}
