namespace RestaurantMS.Web.Models;

public class EmployeeReportViewModel
{
    public DateTime FromDate { get; set; } = DateTime.UtcNow.AddDays(-30);
    public DateTime ToDate { get; set; } = DateTime.UtcNow;
    public IReadOnlyList<EmployeePerformanceRowModel> Employees { get; set; } = Array.Empty<EmployeePerformanceRowModel>();
    public decimal OverallAttendanceRate => Employees.Count > 0 ? Employees.Average(e => e.AttendancePercentage) : 0;
    public decimal TotalSalesContribution => Employees.Sum(e => e.TotalSalesValue);
    public int TotalTablesServed => Employees.Sum(e => e.TablesServed);
}
