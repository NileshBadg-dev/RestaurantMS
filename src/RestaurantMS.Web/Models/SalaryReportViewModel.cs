namespace RestaurantMS.Web.Models;

public class SalaryReportViewModel
{
    public int Month { get; set; } = DateTime.UtcNow.Month;
    public int Year { get; set; } = DateTime.UtcNow.Year;
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    public IReadOnlyList<SalaryRowModel> Salaries { get; set; } = Array.Empty<SalaryRowModel>();
    public decimal TotalGrossSalary => Salaries.Sum(s => s.GrossEarnedSalary);
    public decimal TotalAdvanceDeductions => Salaries.Sum(s => s.AdvanceDeduction);
    public decimal TotalNetPayable => Salaries.Sum(s => s.NetPayableSalary);
    public decimal TotalPaid => Salaries.Where(s => s.PaymentStatus == "Paid").Sum(s => s.NetPayableSalary);
    public decimal TotalPending => Salaries.Where(s => s.PaymentStatus == "Pending").Sum(s => s.NetPayableSalary);
}
