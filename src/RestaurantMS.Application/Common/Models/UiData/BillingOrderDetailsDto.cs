namespace RestaurantMS.Application.Common.Models.UiData;

public record BillingOrderDetailsDto(
    string OrderType,
    string? TableCode,
    decimal Subtotal,
    decimal GstPercent,
    decimal GstAmount,
    decimal TotalAmount,
    string? SuggestedCustomerName,
    string? SuggestedCustomerMobile,
    decimal? OutstandingCredit,
    decimal? CreditLimit,
    IReadOnlyList<BillingItemRowDto> Items);
