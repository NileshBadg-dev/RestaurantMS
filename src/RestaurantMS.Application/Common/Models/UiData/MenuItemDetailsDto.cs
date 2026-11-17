namespace RestaurantMS.Application.Common.Models.UiData;

public record MenuItemDetailsDto(
    long Id,
    long CategoryId,
    string CategoryName,
    string Name,
    string MarathiName,
    decimal Price,
    decimal TaxPercent,
    bool IsVegetarian,
    bool IsAvailable,
    int PreparationMinutes,
    string? VariantsSummary,
    string? ImageUrl);
