namespace RestaurantMS.Application.Common.Models.UiData;

public record TopSellingItemDto(
    string ItemName,
    int QuantitySold,
    string ColorHex);
