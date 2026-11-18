namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterFloorDto(
    long Id,
    string Name,
    string Code,
    string Description,
    int TableCount,
    int SeatingCapacity,
    string Status);
