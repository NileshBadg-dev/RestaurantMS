namespace RestaurantMS.Application.Common.Models.Billing;

public record BillCalculationRequest(
    string OrderType,
    IReadOnlyList<BillLineInputDto> Items,
    decimal DiscountAmount = 0m,
    decimal PackingCharge = 0m,
    decimal GstPercent = 5m);
