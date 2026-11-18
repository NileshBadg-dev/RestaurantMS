using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Web.Models;

public class EmployeeAdvancesViewModel
{
    public EmployeeAdvanceKpisDto Kpis { get; init; } = new(0m, 0m, 0m, 0);
    public IReadOnlyList<EmployeeAdvanceSummaryDto> Employees { get; init; } = [];
    public long SelectedStaffId { get; init; }
    public EmployeeAdvanceSummaryDto? SelectedEmployee { get; init; }
    public IReadOnlyList<AdvanceLedgerEntryDto> LedgerEntries { get; init; } = [];
    public string? Search { get; init; }
    public decimal MaxAdvanceSalaryPercent { get; init; } = 100m;
    public DateTime? AccountingLockDate { get; init; }
    public bool CanGiveAdvance { get; init; } = true;
    public bool CanRecoverAdvance { get; init; } = true;
    public bool CanApproveOverride { get; init; } = true;
}
