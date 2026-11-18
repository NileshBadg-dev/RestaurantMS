using RestaurantMS.Application.Common.Models.Accounting;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Common.Interfaces;

/// <summary>
/// Central double-entry accounting service. Enforces balanced vouchers and locked periods.
/// </summary>
public interface IAccountingService
{
    Task<bool> IsPeriodLockedAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLockDateAsync(CancellationToken cancellationToken = default);
    Task SetLockDateAsync(DateTime? lockDate, CancellationToken cancellationToken = default);
    Task<Result<PostedVoucherDto>> PostVoucherAsync(PostVoucherRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PostedVoucherDto>> GetPostedVouchersAsync(DateTime? fromDate = null, DateTime? toDate = null, string? voucherType = null, CancellationToken cancellationToken = default);
}
