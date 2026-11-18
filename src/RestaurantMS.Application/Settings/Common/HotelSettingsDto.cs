namespace RestaurantMS.Application.Settings.Common;

public class HotelSettingsDto
{
    public string HotelNameEn { get; set; } = string.Empty;
    public string HotelNameMr { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public string? Gstin { get; set; }
    public string? Fssai { get; set; }
    public string Currency { get; set; } = "INR";
    public string Timezone { get; set; } = "Asia/Kolkata";
    public string? ReceiptHeader { get; set; }
    public string? ReceiptFooter { get; set; }
    public string PaperSize { get; set; } = "80mm";
    public bool AutoPrintOnSettle { get; set; } = true;
    public int PrintCopies { get; set; } = 1;
    public bool ShowMarathiItemNames { get; set; } = true;
    public bool ShowUpiQrOnReceipt { get; set; } = true;
    public bool AutoPrintKot { get; set; } = true;
    public string HotelUpiId { get; set; } = "grandrestaurant@okhdfc";
    public bool UpiReferenceRequired { get; set; } = false;
    public bool CardDetailsRequired { get; set; } = true;
}
