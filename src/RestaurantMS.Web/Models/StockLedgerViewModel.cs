using RestaurantMS.Application.Common.Models.Inventory;

namespace RestaurantMS.Web.Models;

public class StockLedgerViewModel
{
    public IReadOnlyList<StockLedgerEntryDto> Entries { get; set; } = [];
    public IReadOnlyList<InventoryStockItemDto> StockItems { get; set; } = [];
    public long? SelectedItemId { get; set; }
    public string MovementType { get; set; } = "All";
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal TotalInQty { get; set; }
    public decimal TotalOutQty { get; set; }
}
