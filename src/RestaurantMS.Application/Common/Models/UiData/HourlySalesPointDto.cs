namespace RestaurantMS.Application.Common.Models.UiData;

public record HourlySalesPointDto(
    string HourLabel,
    decimal DineInAmount,
    decimal ParcelAmount);
