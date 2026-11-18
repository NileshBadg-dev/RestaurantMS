using RestaurantMS.Application.Common.Models.Inventory;

namespace RestaurantMS.Web.Models;

public class LowStockViewModel
{
    public IReadOnlyList<InventoryStockItemDto> AlertItems { get; set; } = [];
    public int CriticalItemCount { get; set; }
    public int OutOfStockCount { get; set; }
    public decimal EstimatedReorderCost { get; set; }
}
