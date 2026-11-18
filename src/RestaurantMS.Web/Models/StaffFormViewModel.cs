namespace RestaurantMS.Web.Models;

public class StaffFormViewModel
{
    public long Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Waiter";
    public string MobileNumber { get; set; } = string.Empty;
    public string Shift { get; set; } = "Morning (07:00 AM - 03:30 PM)";
    public string SalaryType { get; set; } = "Monthly"; // Monthly or Daily
    public decimal MonthlySalary { get; set; }
    public DateTime JoiningDate { get; set; } = DateTime.UtcNow.Date;
    public string IdProof { get; set; } = string.Empty;
    public string BankOrUpi { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
