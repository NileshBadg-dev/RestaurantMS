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

    public string ActiveTab { get; set; } = "profile";
}
