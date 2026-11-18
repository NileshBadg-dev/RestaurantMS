namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterCategoryDto(
    long Id,
    string Name,
    string MarathiName,
    int DisplayOrder,
    int ItemCount,
    string Status);
