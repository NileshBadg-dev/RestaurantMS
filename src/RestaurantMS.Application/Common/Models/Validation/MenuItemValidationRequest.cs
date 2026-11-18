namespace RestaurantMS.Application.Common.Models.Validation;

public record MenuItemValidationRequest(
    string ShortCode,
    string NameEn,
    string NameMr,
    long CategoryId,
    decimal Price,
    decimal GstPercent,
    int PrepTimeMinutes);
