namespace RestaurantMS.Application.Common.Models.Inventory;

public record PurchaseLineInputDto(
    long StockItemId,
    string ItemName,
    decimal Quantity,
    string Unit,
    decimal Rate,
    decimal TaxPercent,
    decimal Amount);
