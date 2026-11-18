namespace RestaurantMS.Application.Common.Models.Staff;

public record AdvanceLedgerEntryDto(
    long Id,
    long StaffId,
    string EmployeeCode,
    string EmployeeName,
    DateTime Date,
    string VoucherNumber,
    string EntryType,
    string PaymentMode,
    decimal Amount,
    decimal RunningBalance,
    string Purpose,
    string? RecoveryPlanSummary,
    string Note);
