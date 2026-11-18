namespace RestaurantMS.Web.Models;

public record SettleBillResponseDto(
    string BillNumber,
    decimal GrandTotal,
    string PrimaryPaymentMode,
    string OrderType,
    string? ParcelToken,
    string ReceiptUrl,
    string? ParcelTokenUrl,
    bool AutoPrint,
    string PaperSize,
    int Copies,
    string WhatsAppText,
    string? VoucherNumber,
    decimal ChangeToReturn,
    decimal CreditAmount,
    decimal NewOutstanding,
    string? CustomerPhone);
