using RestaurantMS.Application.Common.Models.Inventory;

namespace RestaurantMS.Web.Models;

public class StockAdjustmentViewModel
{
    public long StockItemId { get; set; }
    public decimal PhysicalCount { get; set; }
    public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow.AddHours(5).AddMinutes(30).Date;
    public string Reason { get; set; } = string.Empty;
    public bool AllowNegativeStock { get; set; }
    public IReadOnlyList<InventoryStockItemDto> StockItems { get; set; } = [];
    public IReadOnlyList<StockLedgerEntryDto> RecentAdjustments { get; set; } = [];
}
