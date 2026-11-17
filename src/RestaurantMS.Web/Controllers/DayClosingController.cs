using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class DayClosingController : Controller
{
    private readonly IDayClosingUiData _dayClosingUiData;

    public DayClosingController(IDayClosingUiData dayClosingUiData)
    {
        _dayClosingUiData = dayClosingUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var summary = await _dayClosingUiData.GetDayClosingSummaryAsync(null, cancellationToken);

        var viewModel = new DayClosingViewModel
        {
            Summary = summary,
            ActualCashInDrawer = summary.ExpectedDrawerCash,
            IsClosed = summary.IsSettled
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseDay(DayClosingViewModel model, CancellationToken cancellationToken)
    {
        await _dayClosingUiData.CloseDayAsync(model.ActualCashInDrawer, model.ClosingRemarks, cancellationToken);
        TempData["SuccessMessage"] = "Day closed successfully! End of day register locked.";
        return RedirectToAction(nameof(Index));
    }
}
