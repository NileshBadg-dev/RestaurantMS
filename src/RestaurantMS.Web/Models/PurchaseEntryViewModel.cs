namespace RestaurantMS.Web.Models;

public class PurchaseEntryViewModel
{
    public string SupplierId { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public string PaymentMode { get; set; } = "Credit"; // Cash, Credit, Bank
    public string Notes { get; set; } = string.Empty;
    public List<PurchaseItemRowModel> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
}
