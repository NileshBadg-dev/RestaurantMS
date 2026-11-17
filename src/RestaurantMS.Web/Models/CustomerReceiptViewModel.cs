namespace RestaurantMS.Web.Models;

public class CustomerReceiptViewModel
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal CurrentOutstanding { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMode { get; set; } = "Cash"; // Cash, UPI, Card, Cheque
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
}
