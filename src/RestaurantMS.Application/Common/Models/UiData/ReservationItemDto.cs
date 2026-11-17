namespace RestaurantMS.Application.Common.Models.UiData;

public record ReservationItemDto(
    long Id,
    string CustomerName,
    string Mobile,
    string TableCode,
    string ReservationTime,
    int GuestsCount,
    string Status,
    string? SpecialRequest);
