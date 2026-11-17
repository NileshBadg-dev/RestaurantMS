namespace RestaurantMS.Web.Models;

public class VoucherFormViewModel
{
    public string VoucherNumber { get; set; } = string.Empty;
    public string VoucherType { get; set; } = "Journal"; // Payment, Receipt, Journal, Contra
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Narration { get; set; } = string.Empty;
    public List<VoucherEntryLineModel> Lines { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public bool IsBalanced => TotalDebit == TotalCredit && TotalDebit > 0;
}
