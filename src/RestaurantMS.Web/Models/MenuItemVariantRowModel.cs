namespace RestaurantMS.Web.Models;

public class MenuItemVariantRowModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsDefault { get; set; }
}
