using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class MenuCategoriesViewModel
{
    public IReadOnlyList<MenuCategoryDto> Categories { get; set; } = new List<MenuCategoryDto>();
    public int TotalCategories { get; set; }
    public int TotalItems { get; set; }
}
