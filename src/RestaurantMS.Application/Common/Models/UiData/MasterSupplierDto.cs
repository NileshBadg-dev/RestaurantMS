namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterSupplierDto(
    long Id,
    string Name,
    string ContactPerson,
    string Phone,
    string Gstin,
    string Category,
    string Status);
