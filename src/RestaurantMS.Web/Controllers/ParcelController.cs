using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class ParcelController : Controller
{
    private readonly IParcelUiData _parcelUiData;

    public ParcelController(IParcelUiData parcelUiData)
    {
        _parcelUiData = parcelUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var request = new DataTableRequest { Start = 0, Length = 50 };
        var pagedParcels = await _parcelUiData.GetParcelsAsync(request, null, cancellationToken);
        var parcels = pagedParcels.Items;

        var viewModel = new ParcelListViewModel
        {
            ActiveParcels = parcels,
            TotalPreparing = parcels.Count(p => p.Status.Equals("Preparing", StringComparison.OrdinalIgnoreCase)),
            TotalReady = parcels.Count(p => p.Status.Equals("Ready", StringComparison.OrdinalIgnoreCase)),
            TotalDeliveredToday = 22,
            TotalParcelSalesToday = parcels.Sum(p => p.Amount)
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new CreateParcelViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateParcelViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["SuccessMessage"] = $"Parcel order for {model.CustomerName} created! Token #P-{new Random().Next(10, 99)} printed.";
        return RedirectToAction(nameof(Index));
    }
}
