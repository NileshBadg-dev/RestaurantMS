using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models.Inventory;
using RestaurantMS.Domain.Constants;
using RestaurantMS.SharedKernel;
using RestaurantMS.Web.Authorization;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
[FeatureEnabled(FeatureKeys.Inventory)]
public class InventoryController : Controller
{
    private readonly IInventoryService _inventoryService;
    private readonly IMastersUiData _mastersUiData;
    private readonly IFileStorage _fileStorage;
    private readonly ISettingService _settingService;
    private readonly IAuditUiData _auditUiData;

    public InventoryController(
        IInventoryService inventoryService,
        IMastersUiData mastersUiData,
        IFileStorage fileStorage,
        ISettingService settingService,
        IAuditUiData auditUiData)
    {
        _inventoryService = inventoryService;
        _mastersUiData = mastersUiData;
        _fileStorage = fileStorage;
        _settingService = settingService;
        _auditUiData = auditUiData;
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Inventory.View)]
    public async Task<IActionResult> Index(
        [FromQuery] string? search = null,
        [FromQuery] string status = "All",
        [FromQuery] string category = "All",
        CancellationToken cancellationToken = default)
    {
        var itemsResult = await _inventoryService.GetStockItemsAsync(search, status, category, cancellationToken);
        var allItemsResult = await _inventoryService.GetStockItemsAsync(null, null, null, cancellationToken);
        var kpisResult = await _inventoryService.GetSummaryKpisAsync(cancellationToken);
        var taxUnits = await _mastersUiData.GetTaxUnitsAsync(cancellationToken);

        var allItems = allItemsResult.IsSuccess ? allItemsResult.Value : [];
        var categories = allItems.Select(i => i.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c).ToList();

        var viewModel = new StockSummaryViewModel
        {
            Kpis = kpisResult.IsSuccess ? kpisResult.Value : new StockSummaryKpiDto(0, 0m, 0, 0, 0),
            Items = itemsResult.IsSuccess ? itemsResult.Value : [],
            Categories = categories,
            Units = taxUnits.Where(t => t.Type.Contains("Unit", StringComparison.OrdinalIgnoreCase)).ToList(),
            Search = search,
            StatusFilter = status,
            CategoryFilter = category,
            AllowNegativeStock = await IsNegativeStockAllowedAsync(cancellationToken)
        };

        return View(viewModel);
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Inventory.Purchase)]
    public async Task<IActionResult> Purchase([FromQuery] long? itemId = null, CancellationToken cancellationToken = default)
    {
        var viewModel = await BuildPurchaseViewModelAsync(null, itemId, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(Permissions.Inventory.Purchase)]
    public async Task<IActionResult> Purchase(PurchaseEntryViewModel model, string? submitAction = null, CancellationToken cancellationToken = default)
    {
        string? attachmentUrl = model.AttachmentUrl;
        if (model.AttachmentFile is not null && model.AttachmentFile.Length > 0)
        {
            try
            {
                using var stream = model.AttachmentFile.OpenReadStream();
                attachmentUrl = await _fileStorage.SaveDocumentAsync(stream, model.AttachmentFile.FileName, "purchases", cancellationToken);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(model.AttachmentFile), ex.Message);
                var invalidVm = await BuildPurchaseViewModelAsync(model, null, cancellationToken);
                return View(invalidVm);
            }
        }

        var suppliers = await _mastersUiData.GetSuppliersAsync(cancellationToken);
        var supplier = suppliers.FirstOrDefault(s => s.Id == model.SupplierId);
        var supplierName = supplier?.Name ?? model.SupplierName;

        var lines = (model.Items ?? [])
            .Where(l => !string.IsNullOrWhiteSpace(l.ItemName) || l.StockItemId > 0)
            .Select(l => new PurchaseLineInputDto(l.StockItemId, l.ItemName?.Trim() ?? string.Empty, l.Quantity, l.Unit, l.Rate, l.TaxRate, l.Total))
            .ToList();

        var request = new RecordPurchaseRequest(
            model.SupplierId,
            supplierName,
            model.InvoiceNumber,
            model.InvoiceDate,
            model.PaymentType,
            model.PaymentMode,
            model.PaidAmount,
            attachmentUrl,
            model.Notes,
            lines);

        var actor = User.Identity?.Name ?? "Admin";
        var result = await _inventoryService.RecordPurchaseAsync(request, actor, cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            var invalidVm = await BuildPurchaseViewModelAsync(model, null, cancellationToken);
            return View(invalidVm);
        }

        var rec = result.Value;
        await _auditUiData.LogAsync(
            actor,
            "Inventory",
            "Purchase Recorded",
            "Inventory",
            $"Recorded purchase invoice {rec.InvoiceNumber} from {rec.SupplierName} for Rs {rec.GrandTotal:0.00} ({rec.PaymentStatus}). Voucher: {rec.VoucherNumber}",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            "Info",
            cancellationToken);

        TempData["SuccessMessage"] = $"Purchase invoice #{rec.InvoiceNumber} recorded (Rs {rec.GrandTotal:N2}) and posted to supplier ledger & accounts ({rec.VoucherNumber}).";
        TempData["HighlightedRow"] = rec.InvoiceNumber;

        return string.Equals(submitAction, "saveAndNew", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction(nameof(Purchase))
            : RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Inventory.Adjust)]
    public async Task<IActionResult> Adjustment([FromQuery] long? itemId = null, CancellationToken cancellationToken = default)
    {
        var vm = await BuildAdjustmentViewModelAsync(new StockAdjustmentViewModel { StockItemId = itemId ?? 0 }, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(Permissions.Inventory.Adjust)]
    public async Task<IActionResult> Adjustment(StockAdjustmentViewModel model, string? submitAction = null, CancellationToken cancellationToken = default)
    {
        var actor = User.Identity?.Name ?? "Admin";
        var result = await _inventoryService.RecordAdjustmentAsync(
            new StockAdjustmentRequest(model.StockItemId, model.PhysicalCount, model.AdjustmentDate, model.Reason),
            actor,
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            return View(await BuildAdjustmentViewModelAsync(model, cancellationToken));
        }

        var entry = result.Value;
        await _auditUiData.LogAsync(actor, "Inventory", "Stock Adjusted", "Inventory", $"Adjusted {entry.ItemName} to {entry.BalanceQty:0.###} {entry.Unit} ({entry.Reason})", "127.0.0.1", "Info", cancellationToken);

        TempData["SuccessMessage"] = $"Stock adjustment for '{entry.ItemName}' saved. New balance: {entry.BalanceQty:0.###} {entry.Unit}.";
        TempData["HighlightedRow"] = entry.ItemName;

        return string.Equals(submitAction, "saveAndNew", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction(nameof(Adjustment))
            : RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Inventory.Wastage)]
    public async Task<IActionResult> Wastage([FromQuery] long? itemId = null, CancellationToken cancellationToken = default)
    {
        var vm = await BuildWastageViewModelAsync(new StockWastageViewModel { StockItemId = itemId ?? 0 }, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(Permissions.Inventory.Wastage)]
    public async Task<IActionResult> Wastage(StockWastageViewModel model, string? submitAction = null, CancellationToken cancellationToken = default)
    {
        var actor = User.Identity?.Name ?? "Admin";
        var result = await _inventoryService.RecordWastageAsync(
            new StockWastageRequest(model.StockItemId, model.Quantity, model.WastageDate, model.Category, model.Reason),
            actor,
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            return View(await BuildWastageViewModelAsync(model, cancellationToken));
        }

        var entry = result.Value;
        await _auditUiData.LogAsync(actor, "Inventory", "Wastage Logged", "Inventory", $"Wastage {entry.OutQty:0.###} {entry.Unit} of {entry.ItemName} (Rs {entry.ValueImpact:0.00}) - {entry.Reason}", "127.0.0.1", "Warning", cancellationToken);

        TempData["SuccessMessage"] = $"Wastage for '{entry.ItemName}' ({entry.OutQty:0.###} {entry.Unit}, Rs {entry.ValueImpact:N2}) logged and posted to accounts.";
        TempData["HighlightedRow"] = entry.ItemName;

        return string.Equals(submitAction, "saveAndNew", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction(nameof(Wastage))
            : RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Inventory.Consumption)]
    public async Task<IActionResult> Consumption([FromQuery] long? itemId = null, CancellationToken cancellationToken = default)
    {
        var vm = await BuildConsumptionViewModelAsync(new StockConsumptionViewModel { StockItemId = itemId ?? 0 }, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(Permissions.Inventory.Consumption)]
    public async Task<IActionResult> Consumption(StockConsumptionViewModel model, string? submitAction = null, CancellationToken cancellationToken = default)
    {
        var actor = User.Identity?.Name ?? "Admin";
        var result = await _inventoryService.RecordConsumptionAsync(
            new StockConsumptionRequest(model.StockItemId, model.Quantity, model.ConsumptionDate, model.Department, model.ReferenceNote),
            actor,
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            return View(await BuildConsumptionViewModelAsync(model, cancellationToken));
        }

        var entry = result.Value;
        await _auditUiData.LogAsync(actor, "Inventory", "Stock Consumption", "Inventory", $"Issued {entry.OutQty:0.###} {entry.Unit} of {entry.ItemName} ({entry.Reason})", "127.0.0.1", "Info", cancellationToken);

        TempData["SuccessMessage"] = $"Consumption of {entry.OutQty:0.###} {entry.Unit} '{entry.ItemName}' recorded. Remaining: {entry.BalanceQty:0.###} {entry.Unit}.";
        TempData["HighlightedRow"] = entry.ItemName;

        return string.Equals(submitAction, "saveAndNew", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction(nameof(Consumption))
            : RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Inventory.View)]
    public async Task<IActionResult> LowStock(CancellationToken cancellationToken = default)
    {
        var lowResult = await _inventoryService.GetLowStockItemsAsync(cancellationToken);
        var items = lowResult.IsSuccess ? lowResult.Value : [];

        var viewModel = new LowStockViewModel
        {
            AlertItems = items,
            CriticalItemCount = items.Count,
            OutOfStockCount = items.Count(i => i.Status == "Out"),
            EstimatedReorderCost = Math.Round(items.Sum(i => Math.Max(0m, i.ReorderLevel - i.CurrentStock) * i.LastPurchaseRate), 2, MidpointRounding.AwayFromZero)
        };

        return View(viewModel);
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Inventory.View)]
    public async Task<IActionResult> Ledger(
        [FromQuery] long? itemId = null,
        [FromQuery] string movementType = "All",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var ledgerResult = await _inventoryService.GetStockLedgerAsync(itemId, movementType, fromDate, toDate, cancellationToken);
        var itemsResult = await _inventoryService.GetStockItemsAsync(null, null, null, cancellationToken);
        var entries = ledgerResult.IsSuccess ? ledgerResult.Value : [];

        var viewModel = new StockLedgerViewModel
        {
            Entries = entries,
            StockItems = itemsResult.IsSuccess ? itemsResult.Value : [],
            SelectedItemId = itemId,
            MovementType = movementType,
            FromDate = fromDate,
            ToDate = toDate,
            TotalInQty = entries.Sum(e => e.InQty),
            TotalOutQty = entries.Sum(e => e.OutQty)
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Suppliers() => RedirectToActionPermanent("Index", "Masters", new { tab = "suppliers" });

    [HttpGet]
    public IActionResult SupplierForm() => RedirectToActionPermanent("Index", "Masters", new { tab = "suppliers" });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(Permissions.Inventory.Create)]
    public async Task<IActionResult> QuickAddMaster([FromBody] QuickAddMasterItemRequest? request, CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Name))
        {
            return Json(Result.Failure("Masters.NameRequired", "Name is required."));
        }

        var kind = (request.Kind ?? string.Empty).Trim().ToLowerInvariant();
        if (kind == "supplier")
        {
            await _mastersUiData.SaveEntryAsync("suppliers", new Dictionary<string, string>
            {
                ["Name"] = request.Name.Trim(),
                ["ContactPerson"] = request.ContactPerson?.Trim() ?? request.Name.Trim(),
                ["Phone"] = request.Phone?.Trim() ?? "9800000000",
                ["Gstin"] = request.Gstin?.Trim() ?? "-",
                ["Category"] = request.Category?.Trim() ?? "Grocery",
                ["IsActive"] = "true"
            }, cancellationToken);

            var suppliers = await _mastersUiData.GetSuppliersAsync(cancellationToken);
            var created = suppliers.FirstOrDefault(s => s.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase));
            return Json(Result.Success(new { id = created?.Id ?? 0, name = created?.Name ?? request.Name.Trim() }));
        }

        if (kind is "unit" or "tax")
        {
            var typeLabel = kind == "unit" ? "Measurement Unit" : "Tax Slab";
            var rateOrSymbol = string.IsNullOrWhiteSpace(request.CodeOrRate) ? (kind == "unit" ? request.Name.Trim().ToLowerInvariant() : "5%") : request.CodeOrRate.Trim();
            await _mastersUiData.SaveEntryAsync("tax-units", new Dictionary<string, string>
            {
                ["Name"] = request.Name.Trim(),
                ["Type"] = typeLabel,
                ["RateOrSymbol"] = rateOrSymbol,
                ["IsActive"] = "true"
            }, cancellationToken);

            return Json(Result.Success(new { name = request.Name.Trim(), value = rateOrSymbol }));
        }

        if (kind == "stockitem")
        {
            var actor = User.Identity?.Name ?? "Admin";
            var createResult = await _inventoryService.CreateStockItemAsync(
                new CreateStockItemRequest(
                    request.Name.Trim(),
                    request.Category?.Trim() ?? "General",
                    request.Unit?.Trim() ?? "kg",
                    request.OpeningStock,
                    request.ReorderLevel,
                    request.LastPurchaseRate,
                    request.ExpiryDate),
                actor,
                cancellationToken);

            return Json(createResult);
        }

        return Json(Result.Failure("Masters.InvalidKind", "Unknown quick-add target."));
    }

    private async Task<PurchaseEntryViewModel> BuildPurchaseViewModelAsync(PurchaseEntryViewModel? existing, long? preselectItemId, CancellationToken cancellationToken)
    {
        var suppliers = await _mastersUiData.GetSuppliersAsync(cancellationToken);
        var taxUnits = await _mastersUiData.GetTaxUnitsAsync(cancellationToken);
        var stockResult = await _inventoryService.GetStockItemsAsync(null, null, null, cancellationToken);
        var purchasesResult = await _inventoryService.GetPurchasesAsync(null, cancellationToken);
        var ledgerResult = await _inventoryService.GetSupplierLedgerAsync(null, cancellationToken);

        var stockItems = stockResult.IsSuccess ? stockResult.Value : [];
        var vm = existing ?? new PurchaseEntryViewModel
        {
            InvoiceDate = DateTime.UtcNow.AddHours(5).AddMinutes(30).Date,
            PaymentType = "Paid",
            PaymentMode = "Cash"
        };

        if (vm.Items.Count == 0)
        {
            var target = preselectItemId.HasValue
                ? stockItems.FirstOrDefault(i => i.Id == preselectItemId.Value)
                : stockItems.FirstOrDefault();

            vm.Items.Add(target is not null
                ? new PurchaseItemRowModel
                {
                    StockItemId = target.Id,
                    ItemName = target.Name,
                    Quantity = Math.Max(1m, target.ReorderLevel - target.CurrentStock > 0 ? target.ReorderLevel - target.CurrentStock : 10m),
                    Unit = target.Unit,
                    Rate = target.LastPurchaseRate > 0 ? target.LastPurchaseRate : 100m,
                    TaxRate = 5m,
                    Total = 0m
                }
                : new PurchaseItemRowModel { Quantity = 1m, Unit = "kg", TaxRate = 5m });
        }

        vm.Suppliers = suppliers;
        vm.Units = taxUnits.Where(t => t.Type.Contains("Unit", StringComparison.OrdinalIgnoreCase)).ToList();
        vm.TaxRates = taxUnits.Where(t => t.Type.Contains("Tax", StringComparison.OrdinalIgnoreCase)).ToList();
        vm.StockItems = stockItems;
        vm.RecentPurchases = purchasesResult.IsSuccess ? purchasesResult.Value : [];
        vm.SupplierLedger = ledgerResult.IsSuccess ? ledgerResult.Value : [];
        return vm;
    }

    private async Task<StockAdjustmentViewModel> BuildAdjustmentViewModelAsync(StockAdjustmentViewModel model, CancellationToken cancellationToken)
    {
        var stockResult = await _inventoryService.GetStockItemsAsync(null, null, null, cancellationToken);
        var ledgerResult = await _inventoryService.GetStockLedgerAsync(null, "Adjustment", null, null, cancellationToken);
        model.StockItems = stockResult.IsSuccess ? stockResult.Value : [];
        model.RecentAdjustments = ledgerResult.IsSuccess ? ledgerResult.Value.Take(10).ToList() : [];
        model.AllowNegativeStock = await IsNegativeStockAllowedAsync(cancellationToken);
        if (model.AdjustmentDate == default) model.AdjustmentDate = DateTime.UtcNow.AddHours(5).AddMinutes(30).Date;
        return model;
    }

    private async Task<StockWastageViewModel> BuildWastageViewModelAsync(StockWastageViewModel model, CancellationToken cancellationToken)
    {
        var stockResult = await _inventoryService.GetStockItemsAsync(null, null, null, cancellationToken);
        var ledgerResult = await _inventoryService.GetStockLedgerAsync(null, "Wastage", null, null, cancellationToken);
        model.StockItems = stockResult.IsSuccess ? stockResult.Value : [];
        model.RecentWastage = ledgerResult.IsSuccess ? ledgerResult.Value.Take(10).ToList() : [];
        if (model.WastageDate == default) model.WastageDate = DateTime.UtcNow.AddHours(5).AddMinutes(30).Date;
        return model;
    }

    private async Task<StockConsumptionViewModel> BuildConsumptionViewModelAsync(StockConsumptionViewModel model, CancellationToken cancellationToken)
    {
        var stockResult = await _inventoryService.GetStockItemsAsync(null, null, null, cancellationToken);
        var ledgerResult = await _inventoryService.GetStockLedgerAsync(null, "Consumption", null, null, cancellationToken);
        model.StockItems = stockResult.IsSuccess ? stockResult.Value : [];
        model.RecentConsumption = ledgerResult.IsSuccess ? ledgerResult.Value.Take(10).ToList() : [];
        if (model.ConsumptionDate == default) model.ConsumptionDate = DateTime.UtcNow.AddHours(5).AddMinutes(30).Date;
        return model;
    }

    private async Task<bool> IsNegativeStockAllowedAsync(CancellationToken cancellationToken)
    {
        try
        {
            var val = await _settingService.GetSettingAsync("Inventory.AllowNegativeStock", "false", cancellationToken);
            return bool.TryParse(val, out var parsed) && parsed;
        }
        catch
        {
            return false;
        }
    }
}
