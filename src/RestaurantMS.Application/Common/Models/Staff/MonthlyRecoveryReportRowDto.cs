namespace RestaurantMS.Application.Common.Models.Staff;

public record MonthlyRecoveryReportRowDto(
    long StaffId,
    string EmployeeCode,
    string EmployeeName,
    string Designation,
    DateTime RecoveryDate,
    string VoucherNumber,
    string PaymentMode,
    decimal RecoveredAmount,
    decimal BalanceAfterRecovery,
    string Note);
