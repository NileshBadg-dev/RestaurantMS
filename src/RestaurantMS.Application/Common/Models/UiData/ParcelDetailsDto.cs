namespace RestaurantMS.Application.Common.Models.UiData;

public record ParcelDetailsDto(
    string TokenNo,
    string CustomerName,
    string CustomerMobile,
    string? PickupTime,
    decimal PackingCharge,
    decimal Subtotal,
    decimal GstAmount,
    decimal TotalAmount,
    string Status,
    IReadOnlyList<BillingItemRowDto> Items);
