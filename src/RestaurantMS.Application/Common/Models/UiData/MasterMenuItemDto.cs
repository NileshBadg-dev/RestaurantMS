namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterMenuItemDto(
    long Id,
    string Name,
    string Description,
    string Category,
    string DietaryType,
    decimal Price,
    int PrepTimeMinutes,
    string Status);
