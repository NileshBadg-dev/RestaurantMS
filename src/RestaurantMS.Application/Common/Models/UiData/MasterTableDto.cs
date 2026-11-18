namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterTableDto(
    long Id,
    string TableCode,
    string FloorName,
    int Capacity,
    string Shape,
    string Status);
