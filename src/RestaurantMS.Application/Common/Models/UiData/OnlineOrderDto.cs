namespace RestaurantMS.Application.Common.Models.UiData;

public record OnlineOrderDto(
    string OrderId,
    string Channel,
    string CustomerName,
    string CustomerPhone,
    string DeliveryAddress,
    string ItemsSummary,
    decimal TotalAmount,
    decimal CommissionAmount,
    decimal NetPayout,
    string Status,
    string OrderTime,
    int PrepTimeMinutes,
    string? RiderName,
    string? RiderPhone,
    string? RiderOtp);
