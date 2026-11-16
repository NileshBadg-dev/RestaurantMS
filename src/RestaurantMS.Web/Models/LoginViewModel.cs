using System.ComponentModel.DataAnnotations;

namespace RestaurantMS.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "User ID is required.")]
    [Display(Name = "User ID")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
    public string? HotelName { get; set; }
    public string? LogoUrl { get; set; }
    public string? ErrorMessage { get; set; }
}
