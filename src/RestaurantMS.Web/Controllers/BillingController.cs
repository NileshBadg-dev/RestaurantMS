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
public class BillingController : Controller
{
    private readonly IBillingUiData _billingUiData;
    private readonly ITablesUiData _tablesUiData;
    private readonly IAuditUiData _auditUiData;

    public BillingController(IBillingUiData billingUiData, ITablesUiData tablesUiData, IAuditUiData auditUiData)
    {
        _billingUiData = billingUiData;
        _tablesUiData = tablesUiData;
        _auditUiData = auditUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string? table, CancellationToken cancellationToken)
    {
        var targetTable = string.IsNullOrWhiteSpace(table) ? "T1" : table;
        var orderDetails = await _billingUiData.GetBillForTableAsync(targetTable, cancellationToken);
        var request = new DataTableRequest { Start = 0, Length = 100 };
        var pagedBills = await _billingUiData.GetBillsListAsync(request, cancellationToken);
        var todayBills = pagedBills.Items;

        var availableTables = await _tablesUiData.GetTablesAsync(null, cancellationToken);

        var viewModel = new BillingViewModel
        {
            OrderDetails = orderDetails ?? new BillingOrderDetailsDto("Dine-in", targetTable, 0m, 5m, 0m, 0m, "Walk-in Customer", "", 0m, 10000m, new List<BillingItemRowDto>()),
            TodayBills = todayBills,
            TodayTotalBilled = todayBills.Sum(b => b.Amount),
            TodayCashTotal = todayBills.Where(b => b.PaymentMode == "Cash").Sum(b => b.Amount),
            TodayUpiTotal = todayBills.Where(b => b.PaymentMode == "UPI").Sum(b => b.Amount),
            TodayCardTotal = todayBills.Where(b => b.PaymentMode == "Card").Sum(b => b.Amount),
            TodayCreditTotal = todayBills.Where(b => b.PaymentMode == "Credit" || b.PaymentMode == "Udhari").Sum(b => b.Amount)
        };

        ViewBag.AvailableTables = availableTables;
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settle(
        [FromForm] string? orderType,
        [FromForm] string? tableCode,
        [FromForm] string? customerName,
        [FromForm] string? customerPhone,
        [FromForm] string? customerAddress,
        [FromForm] string? paymentMode,
        [FromForm] decimal? grandTotal,
        [FromForm] decimal? paidAmount,
        [FromForm] decimal? creditAmount,
        [FromForm] decimal? discountAmount,
        [FromForm] string? creditNote,
        CancellationToken cancellationToken)
    {
        var mode = string.IsNullOrWhiteSpace(paymentMode) ? "Cash" : paymentMode;
        var type = string.IsNullOrWhiteSpace(orderType) ? "Dine-in" : orderType;
        var total = grandTotal ?? 0m;
        var custName = customerName?.Trim() ?? "Walk-in Customer";
        var custPhone = customerPhone?.Trim() ?? "";
        var custAddress = customerAddress?.Trim() ?? "";
        var table = string.IsNullOrWhiteSpace(tableCode) ? "Counter" : tableCode;

        var isCredit = mode.Equals("Credit", StringComparison.OrdinalIgnoreCase) || mode.Equals("Udhari", StringComparison.OrdinalIgnoreCase);

        if (isCredit)
        {
            if (string.IsNullOrWhiteSpace(custName) || custName.Equals("Walk-in Customer", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Customer selection is mandatory for Udhari (Credit) billing.";
                return RedirectToAction(nameof(Index), new { table });
            }

            if (string.IsNullOrWhiteSpace(custPhone) || custPhone.Length < 10)
            {
                TempData["ErrorMessage"] = "A valid 10-digit customer mobile number is mandatory for Udhari (Credit) billing.";
                return RedirectToAction(nameof(Index), new { table });
            }

            if (string.IsNullOrWhiteSpace(custAddress) || custAddress.Length < 4)
            {
                TempData["ErrorMessage"] = "Customer physical address is mandatory for Udhari (Credit) billing.";
                return RedirectToAction(nameof(Index), new { table });
            }

            // Register or update customer in directory
            SampleCustomersUiData.AddCustomer(custName, custPhone, custAddress);
            var debt = creditAmount ?? total;
            SampleCustomersUiData.RecordCreditDebt(custPhone, debt);
        }

        var tableOrCust = isCredit ? $"{custName} ({custPhone})" : type.Equals("Dine-in", StringComparison.OrdinalIgnoreCase) ? $"Table {table}" : custName;
        var billNo = SampleBillingUiData.RecordSettledBill(type, tableOrCust, total, mode);

        var actor = User.Identity?.Name ?? "Cashier";
        await _auditUiData.LogAsync(
            actor,
            "Cashier",
            "Bill Settled",
            "Billing",
            $"Settled Bill #{billNo} for ₹{total:N2} via {mode} ({type} - {tableOrCust}). Customer: {custName}, Mobile: {custPhone}",
            "127.0.0.1",
            "Info",
            cancellationToken);

        TempData["SuccessMessage"] = $"Bill #{billNo} settled successfully via {mode} for ₹{total:N2}!";
        return RedirectToAction(nameof(Index));
    }
}
