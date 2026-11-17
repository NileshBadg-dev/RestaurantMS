using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IDashboardUiData
{
    Task<DashboardKpisDto> GetKpisAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HourlySalesPointDto>> GetSalesByHourAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopSellingItemDto>> GetTopSellingItemsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesTrendPointDto>> GetLast7DaysSalesAsync(CancellationToken cancellationToken = default);
    Task<TableTimeStatsDto> GetTableTimeStatsAsync(CancellationToken cancellationToken = default);
    Task<DineInVsParcelSplitDto> GetDineInVsParcelSplitAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<ActiveTableItemDto>> GetActiveTablesAsync(DataTableRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<CompletedOrderItemDto>> GetOrdersDoneTodayAsync(DataTableRequest request, CancellationToken cancellationToken = default);
}
