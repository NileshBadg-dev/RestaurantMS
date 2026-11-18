using RestaurantMS.Application.Common.Models.Inventory;

namespace RestaurantMS.Web.Models;

public class StockWastageViewModel
{
    public long StockItemId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime WastageDate { get; set; } = DateTime.UtcNow.AddHours(5).AddMinutes(30).Date;
    public string Category { get; set; } = "Spoilage";
    public string Reason { get; set; } = string.Empty;
    public IReadOnlyList<InventoryStockItemDto> StockItems { get; set; } = [];
    public IReadOnlyList<StockLedgerEntryDto> RecentWastage { get; set; } = [];
}
