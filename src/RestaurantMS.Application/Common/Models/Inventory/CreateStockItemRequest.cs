namespace RestaurantMS.Application.Common.Models.Inventory;

public record CreateStockItemRequest(
    string Name,
    string Category,
    string Unit,
    decimal OpeningStock,
    decimal ReorderLevel,
    decimal LastPurchaseRate,
    DateTime? ExpiryDate);
