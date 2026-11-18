namespace RestaurantMS.Application.Common.Models.Staff;

public record GiveEmployeeAdvanceRequest
{
    public long StaffId { get; init; }
    public DateTime Date { get; init; } = DateTime.UtcNow.Date;
    public decimal Amount { get; init; }
    public string PaymentMode { get; init; } = "Cash";
    public string Purpose { get; init; } = string.Empty;
    public string RecoveryPlan { get; init; } = "OneTime"; // OneTime or MonthlyInstalment
    public decimal? MonthlyInstalmentAmount { get; init; }
    public string? StartMonth { get; init; } // YYYY-MM
    public string? Note { get; init; }
    public bool IsManagerOverride { get; init; }
    public string? ManagerOverrideReason { get; init; }
}
