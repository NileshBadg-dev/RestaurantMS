using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class StockListViewModel
{
    public IReadOnlyList<StockItemDto> Items { get; set; } = Array.Empty<StockItemDto>();
    public int TotalStockItems { get; set; }
    public int LowStockAlertsCount { get; set; }
    public decimal TotalStockValuation { get; set; }
}
