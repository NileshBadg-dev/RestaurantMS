namespace RestaurantMS.Application.Common.Models.Billing;

public record KotPrintItemDto(
    string ItemName,
    string? MarathiName,
    int Quantity,
    string? Notes = null);
