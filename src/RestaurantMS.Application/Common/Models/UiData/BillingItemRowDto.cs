namespace RestaurantMS.Application.Common.Models.UiData;

public record BillingItemRowDto(
    string ItemName,
    int Quantity,
    decimal Rate,
    decimal Amount,
    string? Notes = null);
