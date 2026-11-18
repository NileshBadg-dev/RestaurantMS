namespace RestaurantMS.Application.Common.Models.Inventory;

public record RecordPurchaseRequest(
    long SupplierId,
    string SupplierName,
    string InvoiceNumber,
    DateTime InvoiceDate,
    string PaymentType,
    string PaymentMode,
    decimal PaidAmount,
    string? AttachmentUrl,
    string? Notes,
    IReadOnlyList<PurchaseLineInputDto> Lines);
