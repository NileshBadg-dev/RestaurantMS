using RestaurantMS.Application.Common.Models.Inventory;

namespace RestaurantMS.Web.Models;

public class StockConsumptionViewModel
{
    public long StockItemId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime ConsumptionDate { get; set; } = DateTime.UtcNow.AddHours(5).AddMinutes(30).Date;
    public string Department { get; set; } = "Main Kitchen";
    public string? ReferenceNote { get; set; }
    public IReadOnlyList<InventoryStockItemDto> StockItems { get; set; } = [];
    public IReadOnlyList<StockLedgerEntryDto> RecentConsumption { get; set; } = [];
}
