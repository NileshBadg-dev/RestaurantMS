using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IMenuUiData
{
    Task<IReadOnlyList<MenuCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<MenuItemDetailsDto>> GetMenuItemsAsync(DataTableRequest request, long? categoryId = null, CancellationToken cancellationToken = default);
    Task<MenuItemDetailsDto?> GetMenuItemByIdAsync(long id, CancellationToken cancellationToken = default);
}
