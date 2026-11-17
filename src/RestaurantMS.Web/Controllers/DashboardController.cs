using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardUiData _dashboardUiData;

    public DashboardController(IDashboardUiData dashboardUiData)
    {
        _dashboardUiData = dashboardUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var kpis = await _dashboardUiData.GetKpisAsync(cancellationToken);
        var hourlySales = await _dashboardUiData.GetSalesByHourAsync(cancellationToken);
        var topItems = await _dashboardUiData.GetTopSellingItemsAsync(cancellationToken);
        var salesTrend = await _dashboardUiData.GetLast7DaysSalesAsync(cancellationToken);
        var tableTimeStats = await _dashboardUiData.GetTableTimeStatsAsync(cancellationToken);
        var split = await _dashboardUiData.GetDineInVsParcelSplitAsync(cancellationToken);
        var activeTables = await _dashboardUiData.GetActiveTablesAsync(new DataTableRequest { Length = 20 }, cancellationToken);
        var recentOrders = await _dashboardUiData.GetOrdersDoneTodayAsync(new DataTableRequest { Length = 20 }, cancellationToken);

        var viewModel = new DashboardViewModel
        {
            Kpis = kpis,
            HourlySales = hourlySales,
            TopSellingItems = topItems,
            SalesTrend = salesTrend,
            TableTimeStats = tableTimeStats,
            DineInVsParcel = split,
            ActiveTables = activeTables.Items,
            RecentOrders = recentOrders.Items
        };

        return View(viewModel);
    }
}
