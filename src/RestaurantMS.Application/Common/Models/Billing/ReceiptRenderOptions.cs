namespace RestaurantMS.Application.Common.Models.Billing;

public record ReceiptRenderOptions(
    string HotelName,
    string? HotelNameMr = null,
    string? LogoUrl = null,
    string? Address = null,
    string? Phone = null,
    string? Gstin = null,
    string? Fssai = null,
    string PaperSize = "80mm",
    string? HeaderText = null,
    string? FooterText = null,
    string? HotelUpiId = null,
    bool ShowUpiQr = true,
    bool ShowMarathiNames = true,
    bool IsDuplicate = false,
    int Copies = 1);
