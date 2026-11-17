using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;
using RestaurantMS.Web.SampleData;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrdersUiData _ordersUiData;
    private readonly IMenuUiData _menuUiData;
    private readonly ITablesUiData _tablesUiData;
    private readonly IAuditUiData _auditUiData;

    public OrdersController(IOrdersUiData ordersUiData, IMenuUiData menuUiData, ITablesUiData tablesUiData, IAuditUiData auditUiData)
    {
        _ordersUiData = ordersUiData;
        _menuUiData = menuUiData;
        _tablesUiData = tablesUiData;
        _auditUiData = auditUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var request = new DataTableRequest { Start = 0, Length = 50 };
        var pagedOrders = await _ordersUiData.GetOpenOrdersAsync(request, cancellationToken);
        var orders = pagedOrders.Items;

        var viewModel = new OrdersViewModel
        {
            OpenOrders = orders,
            ActiveDineInOrders = orders.Count(o => !o.TableOrType.Contains("Takeaway") && !o.TableOrType.Contains("Parcel")),
            ActiveParcelOrders = orders.Count(o => o.TableOrType.Contains("Takeaway") || o.TableOrType.Contains("Parcel")),
            TotalRunningRevenue = orders.Sum(o => o.RunningAmount)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> New([FromQuery] string? table, CancellationToken cancellationToken)
    {
        var categories = await _menuUiData.GetCategoriesAsync(cancellationToken);
        var menuItems = await _ordersUiData.GetMenuForOrderingAsync(null, null, cancellationToken);
        var tables = await _tablesUiData.GetTablesAsync(null, cancellationToken);

        var selectedTable = tables.FirstOrDefault(t => t.TableCode.Equals(table, StringComparison.OrdinalIgnoreCase)) ?? tables.FirstOrDefault();
        var selectedTableCode = selectedTable?.TableCode ?? table ?? "T1";
        var sectionName = selectedTable?.Section ?? "AC hall";

        var viewModel = new OrderTakingViewModel
        {
            TableId = selectedTableCode,
            TableName = selectedTableCode,
            SectionName = sectionName,
            WaiterName = "Sachin Patil",
            GuestCount = selectedTable?.Capacity ?? 2,
            AvailableTables = tables,
            Categories = categories,
            AvailableMenuItems = menuItems,
            Subtotal = 0m
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendKot(
        [FromForm] string tableId,
        [FromForm] string? waiterName,
        [FromForm] int? guestCount,
        [FromForm] string? itemsJson,
        [FromForm] string? kitchenNotes,
        CancellationToken cancellationToken)
    {
        var targetTable = string.IsNullOrWhiteSpace(tableId) ? "T1" : tableId;
        var waiter = string.IsNullOrWhiteSpace(waiterName) ? "Sachin Patil" : waiterName;
        var items = new List<BillingItemRowDto>();

        if (!string.IsNullOrWhiteSpace(itemsJson))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<OrderItemSubmissionDto>>(itemsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed != null)
                {
                    foreach (var p in parsed)
                    {
                        var rate = p.Rate;
                        var qty = Math.Max(1, p.Quantity);
                        items.Add(new BillingItemRowDto(p.Name, qty, rate, rate * qty, p.Notes));
                    }
                }
            }
            catch
            {
                // Fallback
            }
        }

        if (items.Count == 0)
        {
            // Default dish if user clicked Send without items
            items.Add(new BillingItemRowDto("Paneer Butter Masala", 1, 280m, 280m, kitchenNotes));
        }

        var billAmount = items.Sum(i => i.Amount);
        var orderId = SampleOrdersUiData.PlaceNewOrder(targetTable, items, waiter, billAmount);

        var actor = User.Identity?.Name ?? "Cashier";
        await _auditUiData.LogAsync(actor, "Cashier", "KOT Generated", "Orders", $"Order #{orderId} placed for {targetTable} with {items.Count} items (₹{billAmount})", "127.0.0.1", "Info", cancellationToken);

        TempData["SuccessMessage"] = $"KOT #{orderId.Replace("ORD-", "KOT-")} generated for Table {targetTable}! Sent to kitchen successfully.";
        return RedirectToAction(nameof(Index));
    }
}
