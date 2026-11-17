using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly IInventoryUiData _inventoryUiData;

    public InventoryController(IInventoryUiData inventoryUiData)
    {
        _inventoryUiData = inventoryUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var stockItems = await _inventoryUiData.GetStockItemsAsync(new DataTableRequest { Length = 100 }, cancellationToken);

        var viewModel = new StockListViewModel
        {
            Items = stockItems.Items,
            TotalStockItems = stockItems.TotalCount,
            LowStockAlertsCount = stockItems.Items.Count(i => i.IsLowStock),
            TotalStockValuation = stockItems.Items.Sum(i => i.CurrentStock * i.CostPrice)
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Purchase()
    {
        var viewModel = new PurchaseEntryViewModel
        {
            Items = new List<PurchaseItemRowModel>
            {
                new() { ItemName = "Basmati Rice", Quantity = 25, Unit = "Kg", Rate = 85, TaxRate = 5, Total = 2231.25m }
            },
            Subtotal = 2125,
            TaxTotal = 106.25m,
            GrandTotal = 2231.25m
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purchase(PurchaseEntryViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _inventoryUiData.RecordPurchaseAsync(model.SupplierId, model.InvoiceNumber, model.GrandTotal, cancellationToken);
        TempData["SuccessMessage"] = $"Purchase invoice #{model.InvoiceNumber} recorded!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Suppliers(CancellationToken cancellationToken)
    {
        var suppliers = await _inventoryUiData.GetSuppliersAsync(new DataTableRequest { Length = 100 }, cancellationToken);

        var viewModel = new SupplierListViewModel
        {
            Suppliers = suppliers.Items,
            TotalSuppliers = suppliers.TotalCount,
            TotalPayable = suppliers.Items.Sum(s => s.PendingPayment)
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult SupplierForm(string? id)
    {
        var viewModel = new SupplierFormViewModel
        {
            Id = id ?? string.Empty
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SupplierForm(SupplierFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _inventoryUiData.SaveSupplierAsync(model.Name, model.Phone, cancellationToken);
        TempData["SuccessMessage"] = $"Supplier '{model.Name}' saved!";
        return RedirectToAction(nameof(Suppliers));
    }

    [HttpGet]
    public async Task<IActionResult> LowStock(CancellationToken cancellationToken)
    {
        var alertItems = await _inventoryUiData.GetLowStockAlertsAsync(cancellationToken);

        var viewModel = new LowStockViewModel
        {
            AlertItems = alertItems,
            CriticalItemCount = alertItems.Count,
            EstimatedReorderCost = alertItems.Sum(i => (i.ReorderLevel - i.CurrentStock) * i.CostPrice)
        };

        return View(viewModel);
    }
}
