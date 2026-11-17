using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class TablesController : Controller
{
    private readonly ITablesUiData _tablesUiData;

    public TablesController(ITablesUiData tablesUiData)
    {
        _tablesUiData = tablesUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string section = "All", CancellationToken cancellationToken = default)
    {
        var tables = await _tablesUiData.GetTablesAsync(section, cancellationToken);
        var sections = await _tablesUiData.GetSectionsAsync(cancellationToken);

        var totalTables = tables.Count;
        var occupiedCount = tables.Count(t => t.Status.Equals("Occupied", StringComparison.OrdinalIgnoreCase));
        var freeCount = tables.Count(t => t.Status.Equals("Free", StringComparison.OrdinalIgnoreCase));
        var reservedCount = tables.Count(t => t.Status.Equals("Reserved", StringComparison.OrdinalIgnoreCase));
        var cleaningCount = tables.Count(t => t.Status.Equals("Cleaning", StringComparison.OrdinalIgnoreCase));
        var runningSales = tables.Sum(t => t.CurrentBillAmount ?? 0);

        var viewModel = new TablesViewModel
        {
            Tables = tables,
            Sections = sections,
            TotalTables = totalTables,
            OccupiedCount = occupiedCount,
            FreeCount = freeCount,
            ReservedCount = reservedCount,
            CleaningCount = cleaningCount,
            TotalRunningSales = runningSales,
            SelectedSection = section
        };

        return View(viewModel);
    }
}
