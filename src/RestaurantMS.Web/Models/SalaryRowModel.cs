namespace RestaurantMS.Web.Models;

public class SalaryRowModel
{
    public long StaffId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal MonthlyBaseSalary { get; set; }
    public int TotalDaysInMonth { get; set; } = 30;
    public int AttendedDays { get; set; } = 28;
    public decimal GrossEarnedSalary { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal OvertimeBonus { get; set; }
    public decimal NetPayableSalary => GrossEarnedSalary + OvertimeBonus - AdvanceDeduction;
    public string PaymentStatus { get; set; } = "Pending"; // Paid, Pending
    public string? PaymentDate { get; set; }
}
