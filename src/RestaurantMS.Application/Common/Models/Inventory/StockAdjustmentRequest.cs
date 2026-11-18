namespace RestaurantMS.Application.Common.Models.Inventory;

public record StockAdjustmentRequest(
    long StockItemId,
    decimal PhysicalCount,
    DateTime AdjustmentDate,
    string Reason);
