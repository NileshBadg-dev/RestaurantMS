namespace RestaurantMS.Application.Common.Models.Inventory;

public record PurchaseRecordDto(
    long Id,
    long SupplierId,
    string SupplierName,
    string InvoiceNumber,
    DateTime InvoiceDate,
    decimal Subtotal,
    decimal TaxTotal,
    decimal GrandTotal,
    string PaymentStatus,
    string PaymentMode,
    decimal PaidAmount,
    decimal BalanceDue,
    string? AttachmentUrl,
    string? VoucherNumber,
    string? Notes,
    IReadOnlyList<PurchaseLineInputDto> Lines);
