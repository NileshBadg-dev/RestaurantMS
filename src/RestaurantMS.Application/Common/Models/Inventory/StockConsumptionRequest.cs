namespace RestaurantMS.Application.Common.Models.Inventory;

public record StockConsumptionRequest(
    long StockItemId,
    decimal Quantity,
    DateTime ConsumptionDate,
    string Department,
    string? ReferenceNote);
