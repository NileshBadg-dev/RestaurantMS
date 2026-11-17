namespace RestaurantMS.Application.Common.Models.UiData;

public record OrderItemMenuDto(
    long Id,
    string ShortCode,
    string Name,
    string MarathiName,
    string Category,
    decimal Price,
    bool IsVegetarian,
    string FoodType, // "Veg", "NonVeg", "Egg"
    string? ImageUrl,
    bool IsAvailable,
    int PreparationMinutes);
