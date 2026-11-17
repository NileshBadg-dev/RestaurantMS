using System.ComponentModel.DataAnnotations;

namespace RestaurantMS.Web.Models;

public class ResetUserPasswordViewModel
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "New Temporary Password")]
    public string NewPassword { get; set; } = string.Empty;
}
