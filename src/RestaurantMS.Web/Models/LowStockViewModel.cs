using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class LowStockViewModel
{
    public IReadOnlyList<StockItemDto> AlertItems { get; set; } = Array.Empty<StockItemDto>();
    public int CriticalItemCount { get; set; }
    public decimal EstimatedReorderCost { get; set; }
}
