using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Authorization;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
[PermissionAuthorize(Permissions.OnlineOrders.View)]
public class OnlineOrdersController : Controller
{
    private readonly IOnlineOrdersUiData _onlineOrdersUiData;

    public OnlineOrdersController(IOnlineOrdersUiData onlineOrdersUiData)
    {
        _onlineOrdersUiData = onlineOrdersUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var orders = await _onlineOrdersUiData.GetOnlineOrdersAsync(cancellationToken);
        var model = new OnlineOrdersViewModel { Orders = orders };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string orderId, string newStatus, CancellationToken cancellationToken)
    {
        await _onlineOrdersUiData.UpdateOrderStatusAsync(orderId, newStatus, cancellationToken);
        TempData["SuccessMessage"] = $"Order {orderId} moved to {newStatus}.";
        return RedirectToAction(nameof(Index));
    }
}
