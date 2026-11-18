using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Web.Models;

public class AdvanceReportsViewModel
{
    public string ReportType { get; init; } = "outstanding"; // ledger, outstanding, recovery
    public long? StaffId { get; init; }
    public int Month { get; init; } = DateTime.UtcNow.Month;
    public int Year { get; init; } = DateTime.UtcNow.Year;
    public IReadOnlyList<EmployeeAdvanceSummaryDto> Employees { get; init; } = [];
    public IReadOnlyList<EmployeeAdvanceSummaryDto> OutstandingRows { get; init; } = [];
    public IReadOnlyList<AdvanceLedgerEntryDto> LedgerRows { get; init; } = [];
    public IReadOnlyList<MonthlyRecoveryReportRowDto> RecoveryRows { get; init; } = [];
    public decimal TotalOutstanding { get; init; }
    public decimal TotalRecovered { get; init; }
}
