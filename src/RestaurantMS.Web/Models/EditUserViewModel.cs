using System.ComponentModel.DataAnnotations;
using RestaurantMS.Application.Roles.Common;

namespace RestaurantMS.Web.Models;

public class EditUserViewModel
{
    public long Id { get; set; }

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

    [Required(ErrorMessage = "Role is required.")]
    [Range(1, long.MaxValue, ErrorMessage = "Please select a valid role.")]
    [Display(Name = "Role")]
    public long RoleId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    public IReadOnlyList<RoleDto> AvailableRoles { get; set; } = [];
}
