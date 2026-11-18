using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Web.Models;

public class StaffProfilePageViewModel
{
    public string ActiveTab { get; init; } = "details"; // details, attendance, advances, salary
    public StaffProfileDto Profile { get; init; } = null!;
    public int SelectedMonth { get; init; } = DateTime.UtcNow.Month;
    public int SelectedYear { get; init; } = DateTime.UtcNow.Year;
    public IReadOnlyList<MonthlyCalendarDayDto> MonthlyCalendar { get; init; } = [];
    public EmployeeAdvanceSummaryDto? AdvanceSummary { get; init; }
    public IReadOnlyList<AdvanceLedgerEntryDto> RecentAdvances { get; init; } = [];
    public IReadOnlyList<SalaryStatementRowDto> SalaryHistory { get; init; } = [];
}
