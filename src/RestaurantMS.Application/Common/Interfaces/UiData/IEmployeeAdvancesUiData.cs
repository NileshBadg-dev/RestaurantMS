using RestaurantMS.Application.Common.Models.Staff;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IEmployeeAdvancesUiData
{
    Task<EmployeeAdvanceKpisDto> GetKpisAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeAdvanceSummaryDto>> GetEmployeeSummariesAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<EmployeeAdvanceSummaryDto?> GetEmployeeSummaryAsync(long staffId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdvanceLedgerEntryDto>> GetEmployeeLedgerAsync(long staffId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdvanceLedgerEntryDto>> GetAllLedgerEntriesAsync(long? staffId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlyRecoveryReportRowDto>> GetMonthlyRecoveryReportAsync(int month, int year, CancellationToken cancellationToken = default);
    Task<decimal> GetMaxAdvancePercentAsync(CancellationToken cancellationToken = default);
    Task<Result<AdvanceLedgerEntryDto>> GiveAdvanceAsync(GiveEmployeeAdvanceRequest request, CancellationToken cancellationToken = default);
    Task<Result<AdvanceLedgerEntryDto>> RecordRecoveryAsync(RecordAdvanceRecoveryRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> SetAutoRecoverAsync(long staffId, bool enabled, CancellationToken cancellationToken = default);
}
