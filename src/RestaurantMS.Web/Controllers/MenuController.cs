using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class MenuController : Controller
{
    private readonly IMenuUiData _menuUiData;

    public MenuController(IMenuUiData menuUiData)
    {
        _menuUiData = menuUiData;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToActionPermanent("Index", "Masters", new { tab = "menu-items" });
    }

    [HttpGet]
    public IActionResult Categories()
    {
        return RedirectToActionPermanent("Index", "Masters", new { tab = "categories" });
    }

    [HttpGet]
    public IActionResult CategoryForm(string? id)
    {
        var viewModel = new CategoryFormViewModel
        {
            Id = id ?? string.Empty
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CategoryForm(CategoryFormViewModel model, string? submitAction = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["SuccessMessage"] = $"Category '{model.NameEn}' saved!";
        TempData["HighlightedRow"] = model.NameEn;
        if (string.Equals(submitAction, "saveAndNew", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CategoryForm));
        }

        return RedirectToActionPermanent("Index", "Masters", new { tab = "categories" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult QuickAddCategory(string nameEn, string? nameMr)
    {
        if (string.IsNullOrWhiteSpace(nameEn))
        {
            return Json(new { isSuccess = false, error = "Category name is required." });
        }

        var trimmed = nameEn.Trim();
        return Json(new { isSuccess = true, value = new { id = trimmed, name = trimmed, marathiName = nameMr?.Trim() ?? trimmed } });
    }

    [HttpGet]
    public IActionResult Items([FromQuery] string? category)
    {
        return RedirectToActionPermanent("Index", "Masters", new { tab = "menu-items", category });
    }

    [HttpGet]
    public IActionResult ItemForm(string? id)
    {
        if (User.IsInRole("Waiter"))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var viewModel = new MenuItemFormViewModel
        {
            Id = id ?? string.Empty
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ItemForm(MenuItemFormViewModel model, string? submitAction = null)
    {
        if (User.IsInRole("Waiter"))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["SuccessMessage"] = $"Dish '{model.NameEn}' saved successfully!";
        TempData["HighlightedRow"] = model.NameEn;
        if (string.Equals(submitAction, "saveAndNew", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(ItemForm));
        }

        return RedirectToActionPermanent("Index", "Masters", new { tab = "menu-items" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteItem(long id)
    {
        if (User.IsInRole("Waiter"))
        {
            return Forbid();
        }

        TempData["SuccessMessage"] = $"Dish #{id} removed successfully from menu!";
        return RedirectToActionPermanent("Index", "Masters", new { tab = "menu-items" });
    }
}
