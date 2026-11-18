namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterTaxUnitDto(
    long Id,
    string Name,
    string Type,
    string RateOrSymbol,
    string Status);
