using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RestaurantMS.Web.Models;

public class HotelSettingsViewModel
{
    [Required(ErrorMessage = "Hotel Name (English) is required.")]
    [StringLength(150, ErrorMessage = "Hotel Name must not exceed 150 characters.")]
    [Display(Name = "Hotel Name (English)")]
    public string HotelNameEn { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hotel Name (Marathi) is required.")]
    [StringLength(150, ErrorMessage = "Hotel Name must not exceed 150 characters.")]
    [Display(Name = "Hotel Name (Marathi)")]
    public string HotelNameMr { get; set; } = string.Empty;

    [Display(Name = "Current Logo")]
    public string? LogoUrl { get; set; }

    [Display(Name = "Upload New Logo (JPG, PNG, WEBP - Max 2MB)")]
    public IFormFile? LogoFile { get; set; }

    [StringLength(500, ErrorMessage = "Address must not exceed 500 characters.")]
    [Display(Name = "Hotel Address")]
    public string? Address { get; set; }

    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Contact number must be 10 digits starting with 6-9.")]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    [EmailAddress(ErrorMessage = "Invalid Email Address.")]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [Display(Name = "GSTIN Number")]
    public string? Gstin { get; set; }

    [RegularExpression(@"^\d{14}$", ErrorMessage = "FSSAI number must be exactly 14 digits.")]
    [Display(Name = "FSSAI License Number")]
    public string? Fssai { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    [Display(Name = "Currency")]
    public string Currency { get; set; } = "INR";

    [Required(ErrorMessage = "Timezone is required.")]
    [Display(Name = "Timezone")]
    public string Timezone { get; set; } = "Asia/Kolkata";

    [StringLength(500, ErrorMessage = "Receipt header must not exceed 500 characters.")]
    [Display(Name = "Receipt Header Text")]
    public string? ReceiptHeader { get; set; }

    [StringLength(500, ErrorMessage = "Receipt footer must not exceed 500 characters.")]
    [Display(Name = "Receipt Footer Text")]
    public string? ReceiptFooter { get; set; }

    [Display(Name = "Receipt Paper Size")]
    public string PaperSize { get; set; } = "80mm";

    [Display(Name = "Auto-Print Receipt on Bill Settle")]
    public bool AutoPrintOnSettle { get; set; } = true;

    [Range(1, 5, ErrorMessage = "Copies must be between 1 and 5.")]
    [Display(Name = "Receipt Copies")]
    public int PrintCopies { get; set; } = 1;

    [Display(Name = "Show Marathi Item Names on Receipt")]
    public bool ShowMarathiItemNames { get; set; } = true;

    [Display(Name = "Show UPI QR Code on Receipt")]
    public bool ShowUpiQrOnReceipt { get; set; } = true;

    [Display(Name = "Auto-Print KOT on Send to Kitchen")]
    public bool AutoPrintKot { get; set; } = true;

    [StringLength(120, ErrorMessage = "Hotel UPI ID must not exceed 120 characters.")]
    [Display(Name = "Hotel UPI ID (VPA)")]
    public string? HotelUpiId { get; set; } = "grandrestaurant@okhdfc";

    [Display(Name = "Require UPI Reference / UTR Number")]
    public bool UpiReferenceRequired { get; set; } = false;

    [Display(Name = "Require Card Last 4 Digits & Approval Code")]
    public bool CardDetailsRequired { get; set; } = true;

    public string ActiveTab { get; set; } = "profile";
}
