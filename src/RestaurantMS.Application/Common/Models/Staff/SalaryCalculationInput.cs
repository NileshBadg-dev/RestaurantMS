namespace RestaurantMS.Application.Common.Models.Staff;

public record SalaryCalculationInput(
    string SalaryType,
    decimal BasicSalary,
    int TotalDaysInMonth,
    int PresentDays,
    int AbsentDays,
    int HalfDays,
    int LeaveDays,
    int WeekOffDays,
    decimal OvertimeHours,
    decimal OvertimeMultiplier,
    decimal OutstandingAdvance,
    decimal PlannedMonthlyInstalment,
    bool AutoRecoverAdvance);
