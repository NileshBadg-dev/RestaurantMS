namespace RestaurantMS.Web.Models;

public class EmployeePerformanceRowModel
{
    public long StaffId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public int ShiftsAttended { get; set; }
    public decimal AttendancePercentage { get; set; }
    public int TablesServed { get; set; }
    public int OrdersPunched { get; set; }
    public decimal TotalSalesValue { get; set; }
    public decimal AverageOrderValue => OrdersPunched > 0 ? TotalSalesValue / OrdersPunched : 0;
    public decimal Rating { get; set; } = 4.8m;
}
