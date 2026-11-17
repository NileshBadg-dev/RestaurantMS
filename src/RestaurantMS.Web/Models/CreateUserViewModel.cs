using System.ComponentModel.DataAnnotations;
using RestaurantMS.Application.Roles.Common;

namespace RestaurantMS.Web.Models;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "User ID is required.")]
    [StringLength(20, MinimumLength = 4, ErrorMessage = "User ID must be between 4 and 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "User ID must contain only letters and digits.")]
    [Display(Name = "User ID")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name must not exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid Email Address.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile number must be 10 digits starting with 6-9.")]
    [Display(Name = "Mobile Number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [Range(1, long.MaxValue, ErrorMessage = "Please select a valid role.")]
    [Display(Name = "Role")]
    public long RoleId { get; set; }

    public IReadOnlyList<RoleDto> AvailableRoles { get; set; } = [];
}
