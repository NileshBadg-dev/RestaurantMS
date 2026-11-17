using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IOrdersUiData
{
    Task<IReadOnlyList<OrderItemMenuDto>> GetMenuForOrderingAsync(string? category = null, string? search = null, CancellationToken cancellationToken = default);
    Task<PagedResult<OpenOrderDto>> GetOpenOrdersAsync(DataTableRequest request, CancellationToken cancellationToken = default);
}
