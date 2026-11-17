using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;
using RestaurantMS.Web.SampleData;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class KitchenController : Controller
{
    private readonly IKitchenUiData _kitchenUiData;

    public KitchenController(IKitchenUiData kitchenUiData)
    {
        _kitchenUiData = kitchenUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string station = "All", CancellationToken cancellationToken = default)
    {
        var displayData = await _kitchenUiData.GetKitchenDisplayAsync(cancellationToken);

        var viewModel = new KitchenDisplayViewModel
        {
            DisplayData = displayData,
            AutoRefreshSeconds = 15,
            SelectedStation = station
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateStatus([FromForm] string ticketId, [FromForm] string newStatus)
    {
        if (newStatus.Equals("Preparing", StringComparison.OrdinalIgnoreCase))
        {
            SampleKitchenUiData.MoveToPreparing(ticketId);
        }
        else if (newStatus.Equals("Ready", StringComparison.OrdinalIgnoreCase))
        {
            SampleKitchenUiData.MoveToReady(ticketId);
        }
        else if (newStatus.Equals("Served", StringComparison.OrdinalIgnoreCase))
        {
            SampleKitchenUiData.ClearServed(ticketId);
        }

        TempData["SuccessMessage"] = $"Ticket #{ticketId} marked as {newStatus}!";
        return RedirectToAction(nameof(Index));
    }
}
