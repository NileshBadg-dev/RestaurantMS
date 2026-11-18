namespace RestaurantMS.Application.Common.Models.Staff;

public record SalaryStatementRowDto(
    long StaffId,
    string EmployeeCode,
    string EmployeeName,
    string Designation,
    string SalaryType,
    int Month,
    int Year,
    int TotalDaysInMonth,
    int PresentDays,
    int AbsentDays,
    int HalfDays,
    decimal OvertimeHours,
    decimal BasicSalary,
    decimal OvertimePay,
    decimal AbsentDeduction,
    decimal OutstandingAdvance,
    decimal AdvanceRecovery,
    decimal NetPayable,
    string Status,
    DateTime? PaidDate,
    string? RecoveryVoucherNumber);
