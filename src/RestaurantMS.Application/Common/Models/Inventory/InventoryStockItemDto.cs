namespace RestaurantMS.Application.Common.Models.Inventory;

public record InventoryStockItemDto(
    long Id,
    string Code,
    string Name,
    string Category,
    string Unit,
    decimal CurrentStock,
    decimal ReorderLevel,
    decimal LastPurchaseRate,
    decimal StockValue,
    DateTime? ExpiryDate,
    bool IsExpiringSoon,
    string Status,
    bool IsActive);
