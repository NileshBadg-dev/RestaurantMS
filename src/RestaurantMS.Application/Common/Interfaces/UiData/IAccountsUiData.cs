using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IAccountsUiData
{
    Task<IReadOnlyList<AccountNodeDto>> GetChartOfAccountsAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<VoucherItemDto>> GetVouchersAsync(DataTableRequest request, string? voucherType = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DayBookEntryDto>> GetDayBookAsync(DateTime? date = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DayBookEntryDto>> GetCashBookAsync(DateTime? date = null, CancellationToken cancellationToken = default);
    Task<FinancialStatementDto> GetTrialBalanceAsync(DateTime? asOf = null, CancellationToken cancellationToken = default);
    Task<FinancialStatementDto> GetProfitLossAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<FinancialStatementDto> GetBalanceSheetAsync(DateTime asOf, CancellationToken cancellationToken = default);
    Task<long> CreateVoucherAsync(string voucherType, decimal totalAmount, string? narration, CancellationToken cancellationToken = default);
}
