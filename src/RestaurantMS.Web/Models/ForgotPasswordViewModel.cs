using System.ComponentModel.DataAnnotations;

namespace RestaurantMS.Web.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "User ID is required.")]
    [Display(Name = "User ID")]
    public string UserId { get; set; } = string.Empty;

    public string? Message { get; set; }
}
