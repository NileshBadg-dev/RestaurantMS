namespace RestaurantMS.Application.Common.Models.UiData;

public record CustomerListItemDto(
    long Id,
    string Name,
    string Mobile,
    string? Address,
    string? Gstin,
    bool CreditAllowed,
    decimal CreditLimit,
    decimal OutstandingBalance);
