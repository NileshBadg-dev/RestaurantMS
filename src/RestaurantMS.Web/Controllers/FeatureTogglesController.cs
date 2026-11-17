using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.FeatureToggles.Commands.SaveFeatureToggles;
using RestaurantMS.Application.FeatureToggles.Queries.GetFeatureToggles;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Authorization;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class FeatureTogglesController : Controller
{
    private readonly ISender _mediator;

    public FeatureTogglesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.FeatureToggles.View)]
    public async Task<IActionResult> Index()
    {
        var result = await _mediator.Send(new GetFeatureTogglesQuery());
        var toggles = result.Value;

        var model = new FeatureTogglesViewModel
        {
            Toggles = toggles,
            ToggleStates = toggles.ToDictionary(t => t.Key, t => t.IsEnabled)
        };

        return View(model);
    }

    [HttpPost]
    [PermissionAuthorize(Permissions.FeatureToggles.Edit)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Dictionary<string, string> toggleInputs)
    {
        var allResult = await _mediator.Send(new GetFeatureTogglesQuery());
        var allToggles = allResult.Value;

        var stateDict = new Dictionary<string, bool>();
        foreach (var toggle in allToggles)
        {
            stateDict[toggle.Key] = toggleInputs.TryGetValue(toggle.Key, out var val) &&
                                    (val == "true" || val == "on" || val == "1");
        }

        var result = await _mediator.Send(new SaveFeatureTogglesCommand(stateDict));
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error.Description;
        }
        else
        {
            TempData["SuccessMessage"] = "Feature toggles updated successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
