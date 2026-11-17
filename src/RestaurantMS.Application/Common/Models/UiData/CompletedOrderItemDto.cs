namespace RestaurantMS.Application.Common.Models.UiData;

public record CompletedOrderItemDto(
    string BillNo,
    string Time,
    string OrderType,
    string TableOrCustomer,
    string ItemsSummary,
    decimal Amount,
    string PaymentMode);
