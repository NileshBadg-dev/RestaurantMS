namespace RestaurantMS.Application.Common.Models.Inventory;

public record StockWastageRequest(
    long StockItemId,
    decimal Quantity,
    DateTime WastageDate,
    string Category,
    string Reason);
