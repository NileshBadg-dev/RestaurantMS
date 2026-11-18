namespace RestaurantMS.Application.Common.Models.Staff;

public record EmployeeAdvanceSummaryDto(
    long StaffId,
    string EmployeeCode,
    string EmployeeName,
    string Designation,
    string? PhotoUrl,
    string SalaryType,
    decimal BasicSalary,
    decimal MonthlyEquivalentSalary,
    bool IsActive,
    decimal TotalGiven,
    decimal TotalRecovered,
    decimal OutstandingBalance,
    decimal PlannedMonthlyInstalment,
    string? NextRecoveryMonth,
    bool AutoRecoverFromSalary);
