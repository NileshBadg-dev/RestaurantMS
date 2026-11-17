namespace RestaurantMS.Application.Common.Models.UiData;

public record TableItemDto(
    string TableCode,
    string Section,
    int Capacity,
    string Status,
    int? ElapsedMinutes,
    decimal? CurrentBillAmount,
    string? ReservedTime);
