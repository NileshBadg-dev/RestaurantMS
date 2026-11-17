using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class MenuItemsViewModel
{
    public IReadOnlyList<MenuItemDetailsDto> Items { get; set; } = new List<MenuItemDetailsDto>();
    public string SelectedCategory { get; set; } = "All";
    public int TotalItems { get; set; }
    public int VegCount { get; set; }
    public int NonVegCount { get; set; }
}
