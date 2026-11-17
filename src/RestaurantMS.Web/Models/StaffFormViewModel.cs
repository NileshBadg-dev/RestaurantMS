namespace RestaurantMS.Web.Models;

public class StaffFormViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Waiter"; // Waiter, Cashier, Kitchen, Manager, Cleaner
    public string MobileNumber { get; set; } = string.Empty;
    public string Shift { get; set; } = "Morning"; // Morning, Evening, Full Day
    public decimal MonthlySalary { get; set; }
    public DateTime JoiningDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
