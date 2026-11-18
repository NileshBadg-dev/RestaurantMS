namespace RestaurantMS.Application.Common.Models.Staff;

public record RecordAdvanceRecoveryRequest
{
    public long StaffId { get; init; }
    public DateTime Date { get; init; } = DateTime.UtcNow.Date;
    public decimal Amount { get; init; }
    public string PaymentMode { get; init; } = "Cash"; // Cash, Bank, UPI, SalaryDeduction
    public string? Note { get; init; }
}
