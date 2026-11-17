namespace RestaurantMS.Application.Common.Models.UiData;

public record ParcelItemDto(
    string TokenNo,
    string CustomerName,
    string CustomerMobile,
    string ItemsSummary,
    decimal Amount,
    string Status,
    string OrderedTime,
    string? PickupTime);
