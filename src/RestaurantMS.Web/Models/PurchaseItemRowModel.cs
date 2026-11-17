namespace RestaurantMS.Web.Models;

public class PurchaseItemRowModel
{
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public string Unit { get; set; } = "Kg";
    public decimal Rate { get; set; }
    public decimal TaxRate { get; set; } = 5.0m;
    public decimal Total { get; set; }
}
