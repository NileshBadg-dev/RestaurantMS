using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Web.Models;

public class StaffHubViewModel
{
    public string ActiveTab { get; init; } = "list"; // list, attendance, salary
    public string? Search { get; init; }

    // KPI summaries
    public int TotalStaff { get; init; }
    public int ActiveStaff { get; init; }
    public int PresentToday { get; init; }
    public decimal MonthlyPayrollEstimate { get; init; }

    // Tab 1: Staff List
    public IReadOnlyList<StaffProfileDto> StaffProfiles { get; init; } = [];

    // Tab 2: Attendance
    public DateTime SelectedDate { get; init; } = DateTime.UtcNow.Date;
    public int SelectedMonth { get; init; } = DateTime.UtcNow.Month;
    public int SelectedYear { get; init; } = DateTime.UtcNow.Year;
    public long SelectedCalendarStaffId { get; init; }
    public bool IsMonthLocked { get; init; }
    public IReadOnlyList<AttendanceEntryDto> DailyAttendance { get; init; } = [];
    public IReadOnlyList<MonthlyCalendarDayDto> MonthlyCalendar { get; init; } = [];

    // Tab 3: Salary Statement
    public bool AutoRecoverAdvance { get; init; } = true;
    public IReadOnlyList<SalaryStatementRowDto> SalaryRows { get; init; } = [];
    public decimal TotalNetPayroll { get; init; }
    public decimal TotalAdvanceRecovery { get; init; }
}
