namespace RestaurantMS.Application.Common.Models.Billing;

public record SettleBillRequest(
    string IdempotencyKey,
    string OrderType,
    string? TableCode,
    string? ParcelToken,
    string? WaiterName,
    IReadOnlyList<BillLineInputDto> Items,
    decimal DiscountAmount,
    decimal PackingCharge,
    decimal GstPercent,
    string PaymentMode,
    IReadOnlyList<BillPaymentLineInputDto> Payments,
    long? CustomerId = null,
    string? CustomerName = null,
    string? CustomerPhone = null,
    string? CustomerAddress = null,
    decimal CustomerOutstanding = 0m,
    decimal CustomerCreditLimit = 0m,
    bool CustomerCreditAllowed = true,
    bool IsManagerOverride = false,
    string? ManagerOverrideReason = null,
    string? CreditNote = null);
