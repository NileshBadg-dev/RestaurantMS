namespace RestaurantMS.Application.Common.Models.UiData;

public record MenuCategoryDto(
    long Id,
    string Name,
    string MarathiName,
    int DisplayOrder,
    int ItemCount,
    bool IsActive);
