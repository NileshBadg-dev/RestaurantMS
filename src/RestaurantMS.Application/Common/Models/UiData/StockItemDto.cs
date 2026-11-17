namespace RestaurantMS.Application.Common.Models.UiData;

public record StockItemDto(
    long Id,
    string Name,
    string Category,
    decimal CurrentStock,
    string Unit,
    decimal ReorderLevel,
    decimal CostPrice,
    bool IsLowStock);
