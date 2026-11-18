namespace RestaurantMS.Application.Common.Models.Staff;

public record SalaryCalculationResult(
    decimal BaseSalaryForMonth,
    decimal OvertimePay,
    decimal AbsentDeduction,
    decimal AdvanceRecovery,
    decimal NetPayable);
