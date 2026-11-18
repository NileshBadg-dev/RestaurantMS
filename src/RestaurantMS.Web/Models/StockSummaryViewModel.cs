using RestaurantMS.Application.Common.Models.Inventory;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class StockSummaryViewModel
{
    public StockSummaryKpiDto Kpis { get; set; } = new(0, 0m, 0, 0, 0);
    public IReadOnlyList<InventoryStockItemDto> Items { get; set; } = [];
    public IReadOnlyList<string> Categories { get; set; } = [];
    public IReadOnlyList<MasterTaxUnitDto> Units { get; set; } = [];
    public string? Search { get; set; }
    public string StatusFilter { get; set; } = "All";
    public string CategoryFilter { get; set; } = "All";
    public bool AllowNegativeStock { get; set; }
}
