using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class ReservationsController : Controller
{
    private readonly IReservationsUiData _reservationsUiData;

    public ReservationsController(IReservationsUiData reservationsUiData)
    {
        _reservationsUiData = reservationsUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var reservations = await _reservationsUiData.GetAllReservationsAsync(cancellationToken);

        var viewModel = new ReservationListViewModel
        {
            Reservations = reservations,
            TodayReservationsCount = reservations.Count,
            ConfirmedCount = reservations.Count(r => r.Status == "Confirmed"),
            PendingCount = reservations.Count(r => r.Status == "Pending")
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new ReservationFormViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _reservationsUiData.CreateReservationAsync(model.CustomerName, model.MobileNumber, model.TableNumber, model.ReservationTime, model.GuestCount, model.Notes, cancellationToken);
        TempData["SuccessMessage"] = $"Reservation confirmed for {model.CustomerName} at Table {model.TableNumber}!";
        return RedirectToAction(nameof(Index));
    }
}
