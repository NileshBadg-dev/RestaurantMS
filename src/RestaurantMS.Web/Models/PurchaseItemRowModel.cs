namespace RestaurantMS.Web.Models;

public class PurchaseItemRowModel
{
    public long StockItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public string Unit { get; set; } = "kg";
    public decimal Rate { get; set; }
    public decimal TaxRate { get; set; } = 5m;
    public decimal Total { get; set; }
}
