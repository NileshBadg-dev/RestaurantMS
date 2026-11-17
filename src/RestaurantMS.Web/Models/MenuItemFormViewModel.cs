namespace RestaurantMS.Web.Models;

public class MenuItemFormViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameMr { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; } = 5.0m;
    public bool IsVegetarian { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public string KitchenStation { get; set; } = "Main";
}
