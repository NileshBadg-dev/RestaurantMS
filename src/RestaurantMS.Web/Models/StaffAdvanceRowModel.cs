namespace RestaurantMS.Web.Models;

public class StaffAdvanceRowModel
{
    public long Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public string DateFormatted { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal AdvanceAmount { get; set; }
    public decimal RecoveredAmount { get; set; }
    public decimal BalanceAmount => AdvanceAmount - RecoveredAmount;
    public string PaymentMode { get; set; } = "Cash";
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, PartiallyDeducted, Settled
}
