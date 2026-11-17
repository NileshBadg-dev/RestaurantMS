namespace RestaurantMS.Web.Models;

public class StaffAttendanceRowModel
{
    public long StaffId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Shift { get; set; } = string.Empty;
    public string Status { get; set; } = "Present"; // Present, Absent, HalfDay, Leave
    public string InTime { get; set; } = "10:00 AM";
    public string OutTime { get; set; } = "04:00 PM";
    public decimal HoursWorked { get; set; } = 6.0m;
    public string? Remarks { get; set; }
}
