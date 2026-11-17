namespace RestaurantMS.Web.Models;

public class StaffDetailsViewModel
{
    public long Id { get; set; } = 1;
    public string EmployeeCode { get; set; } = "EMP-001";
    public string FullName { get; set; } = "Sachin Patil";
    public string Role { get; set; } = "Head Waiter";
    public string MobileNumber { get; set; } = "9822012345";
    public string Email { get; set; } = "sachin.patil@restaurant.local";
    public string Shift { get; set; } = "Morning (10:00 AM - 04:00 PM)";
    public decimal MonthlySalary { get; set; } = 18000m;
    public decimal TotalAdvanceBalance { get; set; } = 2500m;
    public DateTime JoiningDate { get; set; } = new(2024, 3, 15);
    public string BankAccountOrUpi { get; set; } = "sachin@okaxis / SBIN0001234";
    public string AadhaarNumber { get; set; } = "XXXX-XXXX-4812";
    public string EmergencyContact { get; set; } = "Pravin Patil (Brother) - 9822998877";
    public int DaysPresentThisMonth { get; set; } = 24;
    public int DaysAbsentThisMonth { get; set; } = 2;
    public decimal TotalSalesHandled { get; set; } = 142500m;
}
