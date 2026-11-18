namespace RestaurantMS.Web.Models;

public class QuickAddMasterItemRequest
{
    public string Kind { get; set; } = string.Empty; // supplier | unit | tax | stockItem
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Gstin { get; set; }
    public string? Category { get; set; }
    public string? CodeOrRate { get; set; }
    public string? Unit { get; set; }
    public decimal OpeningStock { get; set; }
    public decimal ReorderLevel { get; set; } = 10m;
    public decimal LastPurchaseRate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
