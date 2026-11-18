namespace RestaurantMS.Web.Models;

public class MenuItemFormViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameMr { get; set; } = string.Empty;
    public string CategoryId { get; set; } = "Main Course";
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; } = 5.0m;
    public bool IsTaxInclusive { get; set; }
    public string DietaryType { get; set; } = "Veg"; // Veg, NonVeg, Egg
    public bool IsVegetarian { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public int PrepTimeMinutes { get; set; } = 15;
    public string OrderTypeScope { get; set; } = "All"; // All, DineIn, Parcel
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
    public int MainImageIndex { get; set; }
    public string KitchenStation { get; set; } = "Main";
    public List<MenuItemVariantRowModel> Variants { get; set; } =
    [
        new() { Name = "Half", Price = 160m, IsDefault = false },
        new() { Name = "Full", Price = 280m, IsDefault = true }
    ];
    public List<MenuItemAddonRowModel> Addons { get; set; } =
    [
        new() { Name = "Extra Butter", Price = 30m },
        new() { Name = "Extra Cheese", Price = 45m }
    ];
}
