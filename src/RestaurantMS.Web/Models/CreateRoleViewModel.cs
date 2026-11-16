using System.ComponentModel.DataAnnotations;

namespace RestaurantMS.Web.Models;

public class CreateRoleViewModel
{
    [Required(ErrorMessage = "Role name is required.")]
    [StringLength(50, ErrorMessage = "Role name must not exceed 50 characters.")]
    [Display(Name = "Role Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Description must not exceed 250 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }
}
