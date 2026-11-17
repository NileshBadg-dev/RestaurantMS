using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class DashboardViewModel
{
    public DashboardKpisDto Kpis { get; set; } = default!;
    public IReadOnlyList<HourlySalesPointDto> HourlySales { get; set; } = Array.Empty<HourlySalesPointDto>();
    public IReadOnlyList<TopSellingItemDto> TopSellingItems { get; set; } = Array.Empty<TopSellingItemDto>();
    public IReadOnlyList<SalesTrendPointDto> SalesTrend { get; set; } = Array.Empty<SalesTrendPointDto>();
    public TableTimeStatsDto TableTimeStats { get; set; } = default!;
    public DineInVsParcelSplitDto DineInVsParcel { get; set; } = default!;
    public IReadOnlyList<ActiveTableItemDto> ActiveTables { get; set; } = Array.Empty<ActiveTableItemDto>();
    public IReadOnlyList<CompletedOrderItemDto> RecentOrders { get; set; } = Array.Empty<CompletedOrderItemDto>();
}
