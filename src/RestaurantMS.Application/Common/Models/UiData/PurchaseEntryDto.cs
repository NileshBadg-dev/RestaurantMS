namespace RestaurantMS.Application.Common.Models.UiData;

public record PurchaseEntryDto(
    long Id,
    string InvoiceNo,
    string SupplierName,
    string InvoiceDate,
    decimal TotalAmount,
    int ItemsCount,
    string PaymentStatus);
