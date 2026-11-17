namespace RestaurantMS.Application.Common.Models.UiData;

public record BillSummaryRowDto(
    string BillNo,
    string DateTimeFormatted,
    string OrderType,
    string TableOrCustomer,
    decimal Amount,
    string PaymentMode,
    string Status);
