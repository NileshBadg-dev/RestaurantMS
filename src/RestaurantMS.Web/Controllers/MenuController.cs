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
    public async Task<IActionResult> Categories(CancellationToken cancellationToken)
    {
        var categories = await _menuUiData.GetCategoriesAsync(cancellationToken);

        var viewModel = new MenuCategoriesViewModel
        {
            Categories = categories,
            TotalCategories = categories.Count,
            TotalItems = categories.Sum(c => c.ItemCount)
        };

        return View(viewModel);
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
    public IActionResult CategoryForm(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["SuccessMessage"] = $"Category '{model.NameEn}' saved!";
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet]
    public async Task<IActionResult> Items([FromQuery] string? category, CancellationToken cancellationToken)
    {
        var request = new DataTableRequest { Start = 0, Length = 50 };
        var pagedItems = await _menuUiData.GetMenuItemsAsync(request, null, cancellationToken);
        var items = pagedItems.Items;

        var viewModel = new MenuItemsViewModel
        {
            Items = items,
            SelectedCategory = category ?? "All",
            TotalItems = items.Count,
            VegCount = items.Count(i => i.IsVegetarian),
            NonVegCount = items.Count(i => !i.IsVegetarian)
        };

        return View(viewModel);
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
    public IActionResult ItemForm(MenuItemFormViewModel model)
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
        return RedirectToAction(nameof(Items));
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
        return RedirectToAction(nameof(Items));
    }
}
