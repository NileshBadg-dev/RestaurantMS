namespace RestaurantMS.Application.Common.Models.Billing;

public record BillLineInputDto(
    string ItemName,
    string? MarathiName,
    int Quantity,
    decimal Rate,
    decimal TaxPercent = 5m,
    string? Notes = null);
