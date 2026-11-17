namespace RestaurantMS.Application.Common.Models.UiData;

public record TableSectionDto(
    string Name,
    int TotalTables,
    int OccupiedTables);
