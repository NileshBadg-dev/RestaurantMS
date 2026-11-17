namespace RestaurantMS.Web.Models;

public class VoucherEntryLineModel
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Narration { get; set; } = string.Empty;
}
