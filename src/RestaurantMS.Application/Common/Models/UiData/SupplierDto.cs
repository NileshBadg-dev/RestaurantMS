namespace RestaurantMS.Application.Common.Models.UiData;

public record SupplierDto(
    long Id,
    string Name,
    string ContactPerson,
    string Mobile,
    string? Gstin,
    decimal PendingPayment);
