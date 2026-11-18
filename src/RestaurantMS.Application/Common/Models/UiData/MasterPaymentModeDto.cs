namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterPaymentModeDto(
    long Id,
    string Name,
    string Code,
    decimal FeePercent,
    string Status);
