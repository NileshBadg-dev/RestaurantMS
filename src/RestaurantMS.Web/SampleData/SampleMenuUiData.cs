using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleMenuUiData : IMenuUiData
{
    private static readonly List<MenuCategoryDto> CategoriesList =
    [
        new(1, "Main Course", "मुख्य जेवण", 1, 12, true),
        new(2, "Biryani & Rice", "बिर्याणी आणि राईस", 2, 8, true),
        new(3, "Breads", "रोटी आणि नान", 3, 6, true),
        new(4, "South Indian", "दक्षिण भारतीय", 4, 10, true),
        new(5, "Beverages", "शीतपेये", 5, 5, true),
        new(6, "Desserts", "मिष्टान्न", 6, 4, true)
    ];

    private static readonly List<MenuItemDetailsDto> MenuItemsList =
    [
        new(1, 1, "Main Course", "Paneer Butter Masala", "पनीर बटर मसाला", 280m, 5.0m, true, true, 15, "Half, Full", "/images/paneer.jpg"),
        new(2, 2, "Biryani & Rice", "Chicken Biryani", "चिकन बिर्याणी", 320m, 5.0m, false, true, 20, "Single, Family Pack", "/images/chicken_biryani.jpg"),
        new(3, 2, "Biryani & Rice", "Veg Biryani", "व्हेज बिर्याणी", 220m, 5.0m, true, true, 15, "Single, Family Pack", "/images/veg_biryani.jpg"),
        new(4, 3, "Breads", "Butter Naan", "बटर नान", 45m, 5.0m, true, true, 5, "Plain, Garlic", "/images/naan.jpg"),
        new(5, 3, "Breads", "Tandoori Roti", "तंदूरी रोटी", 25m, 5.0m, true, true, 5, "Plain, Butter", "/images/roti.jpg"),
        new(6, 4, "South Indian", "Masala Dosa", "मसाला डोसा", 90m, 5.0m, true, true, 10, "Normal, Cheese", "/images/dosa.jpg"),
        new(7, 4, "South Indian", "Idli Sambar", "इडली सांबार", 60m, 5.0m, true, true, 5, "2 Pieces", "/images/idli.jpg"),
        new(8, 5, "Beverages", "Sweet Lassi", "गोड लस्सी", 80m, 5.0m, true, true, 5, "Regular, Mango", "/images/lassi.jpg"),
        new(9, 1, "Main Course", "Dal Tadka", "दाल तडका", 180m, 5.0m, true, true, 12, null, "/images/dal.jpg"),
        new(10, 6, "Desserts", "Gulab Jamun", "गुलाब जामुन", 70m, 5.0m, true, true, 3, "2 Pieces", "/images/gulab_jamun.jpg")
    ];

    public Task<IReadOnlyList<MenuCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MenuCategoryDto>>(CategoriesList);
    }

    public Task<PagedResult<MenuItemDetailsDto>> GetMenuItemsAsync(DataTableRequest request, long? categoryId = null, CancellationToken cancellationToken = default)
    {
        var all = MenuItemsList.AsEnumerable();
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            all = all.Where(m => m.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim().ToLowerInvariant();
            all = all.Where(m => m.Name.ToLowerInvariant().Contains(search) ||
                                 m.MarathiName.Contains(search) ||
                                 m.CategoryName.ToLowerInvariant().Contains(search));
        }

        var list = all.ToList();
        var paged = list.Skip(request.Start).Take(request.PageSize).ToList();
        return Task.FromResult(new PagedResult<MenuItemDetailsDto>(paged, list.Count, request.PageNumber, request.PageSize));
    }

    public Task<MenuItemDetailsDto?> GetMenuItemByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = MenuItemsList.FirstOrDefault(m => m.Id == id) ?? MenuItemsList.First();
        return Task.FromResult<MenuItemDetailsDto?>(item);
    }
}
