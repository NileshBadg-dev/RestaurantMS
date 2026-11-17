namespace RestaurantMS.Application.Common.Models.UiData;

public record DineInVsParcelSplitDto(
    int DineInPercentage,
    int ParcelPercentage,
    decimal DineInAverageBill,
    decimal ParcelAverageBill);
