namespace RestaurantMS.Application.Common.Models.Billing;

public record BillPaymentLineInputDto(
    string Mode,
    decimal Amount,
    decimal? CashReceived = null,
    string? UpiReference = null,
    bool UpiConfirmed = false,
    string? CardLast4 = null,
    string? CardApprovalCode = null,
    string? CardBankName = null);
