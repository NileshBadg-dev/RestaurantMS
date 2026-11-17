using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportsUiData _reportsUiData;

    public ReportsController(IReportsUiData reportsUiData)
    {
        _reportsUiData = reportsUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string? category, CancellationToken cancellationToken)
    {
        var reports = await _reportsUiData.GetReportHubCardsAsync(cancellationToken);

        if (!string.IsNullOrEmpty(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            reports = reports.Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var viewModel = new ReportsHubViewModel
        {
            Reports = reports,
            SelectedCategory = category ?? "All"
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ViewReport([FromQuery] string code, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var reportCode = !string.IsNullOrEmpty(code) ? code : "RPT01";
        var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to ?? DateTime.UtcNow;

        var result = await _reportsUiData.GetReportDataAsync(reportCode, fromDate, toDate, null, cancellationToken);

        var viewModel = new ReportDetailViewModel
        {
            ReportCode = reportCode,
            ReportTitle = result.Title,
            FromDate = fromDate,
            ToDate = toDate,
            Result = result
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult ProfitLoss([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var viewModel = new ProfitLossReportViewModel
        {
            FromDate = from ?? DateTime.UtcNow.AddMonths(-1),
            ToDate = to ?? DateTime.UtcNow
        };

        return View(viewModel);
    }
}
