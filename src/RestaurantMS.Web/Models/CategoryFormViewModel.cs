namespace RestaurantMS.Web.Models;

public class CategoryFormViewModel
{
    public string Id { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameMr { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-cup-hot";
    public int DisplayOrder { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
