using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.Billing;
using RestaurantMS.Application.Common.Models.UiData;
using RestaurantMS.SharedKernel;
using RestaurantMS.Web.Models;
using RestaurantMS.Web.SampleData;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class BillingController : Controller
{
    private static readonly Regex MobileRegex = new(@"^[6-9]\d{9}$", RegexOptions.Compiled);

    private readonly IBillingUiData _billingUiData;
    private readonly ITablesUiData _tablesUiData;
    private readonly IMenuUiData _menuUiData;
    private readonly IAuditUiData _auditUiData;
    private readonly IBillingService _billingService;
    private readonly ISettingService _settingService;

    public BillingController(
        IBillingUiData billingUiData,
        ITablesUiData tablesUiData,
        IMenuUiData menuUiData,
        IAuditUiData auditUiData,
        IBillingService billingService,
        ISettingService settingService)
    {
        _billingUiData = billingUiData;
        _tablesUiData = tablesUiData;
        _menuUiData = menuUiData;
        _auditUiData = auditUiData;
        _billingService = billingService;
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string? table, [FromQuery] string? type, CancellationToken cancellationToken)
    {
        var targetTable = string.IsNullOrWhiteSpace(table) ? "T1" : table.Trim();
        var orderDetails = await _billingUiData.GetBillForTableAsync(targetTable, cancellationToken)
            ?? new BillingOrderDetailsDto("Dine-in", targetTable, 0m, 5m, 0m, 0m, "Walk-in Customer", "", 0m, 10000m, []);

        if (!string.IsNullOrWhiteSpace(type) &&
            (type.Equals("Parcel", StringComparison.OrdinalIgnoreCase) || type.Equals("Delivery", StringComparison.OrdinalIgnoreCase)))
        {
            orderDetails = orderDetails with { OrderType = type, TableCode = string.Empty };
        }

        var request = new DataTableRequest { Start = 0, Length = 100 };
        var pagedBills = await _billingUiData.GetBillsListAsync(request, cancellationToken);
        var todayBills = pagedBills.Items;
        var availableTables = await _tablesUiData.GetTablesAsync(null, cancellationToken);
        var menuPaged = await _menuUiData.GetMenuItemsAsync(new DataTableRequest { Start = 0, Length = 200 }, null, cancellationToken);
        var settings = await GetSafeSettingsAsync(cancellationToken);

        var viewModel = new BillingViewModel
        {
            OrderDetails = orderDetails,
            TodayBills = todayBills,
            MenuCatalog = menuPaged.Items,
            Customers = SampleCustomersUiData.GetAllCustomers(),
            TodayTotalBilled = todayBills.Sum(b => b.Amount),
            TodayCashTotal = todayBills.Where(b => b.PaymentMode.Equals("Cash", StringComparison.OrdinalIgnoreCase)).Sum(b => b.Amount),
            TodayUpiTotal = todayBills.Where(b => b.PaymentMode.Equals("UPI", StringComparison.OrdinalIgnoreCase)).Sum(b => b.Amount),
            TodayCardTotal = todayBills.Where(b => b.PaymentMode.Equals("Card", StringComparison.OrdinalIgnoreCase)).Sum(b => b.Amount),
            TodayCreditTotal = todayBills.Where(b => b.PaymentMode.Equals("Credit", StringComparison.OrdinalIgnoreCase) || b.PaymentMode.Equals("Udhari", StringComparison.OrdinalIgnoreCase)).Sum(b => b.Amount),
            HotelName = settings.GetValueOrDefault("HotelNameEn", "Grand Restaurant"),
            HotelUpiId = settings.GetValueOrDefault("Payments.UpiId", "grandrestaurant@okhdfc"),
            UpiReferenceRequired = ParseBool(settings.GetValueOrDefault("Payments.UpiReferenceRequired"), false),
            CardDetailsRequired = ParseBool(settings.GetValueOrDefault("Payments.CardDetailsRequired"), true),
            AutoPrintOnSettle = ParseBool(settings.GetValueOrDefault("Printing.AutoPrintOnSettle"), true),
            PaperSize = settings.GetValueOrDefault("Printing.PaperSize", "80mm"),
            CanOverrideCreditLimit = User.IsInRole("Admin") || User.IsInRole("Owner") || User.IsInRole("Manager") || User.Identity?.IsAuthenticated == true
        };

        ViewBag.AvailableTables = availableTables;
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Calculate([FromBody] BillCalculationRequest? request)
    {
        if (request is null)
        {
            return BadRequest(Result.Failure("Billing.InvalidRequest", "Invalid calculation payload."));
        }

        var result = _billingService.CalculateTotals(request);
        return Json(Result.Success(result));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settle([FromBody] SettleBillRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(Result.Failure("Billing.InvalidRequest", "Invalid settlement payload."));
        }

        var settings = await GetSafeSettingsAsync(cancellationToken);
        var policy = new BillingValidationPolicyDto(
            UpiReferenceRequired: ParseBool(settings.GetValueOrDefault("Payments.UpiReferenceRequired"), false),
            CardDetailsRequired: ParseBool(settings.GetValueOrDefault("Payments.CardDetailsRequired"), true),
            HotelUpiId: settings.GetValueOrDefault("Payments.UpiId", "grandrestaurant@okhdfc"),
            HotelName: settings.GetValueOrDefault("HotelNameEn", "Grand Restaurant"));

        // Rehydrate authoritative customer record from store when selected
        CustomerListItemDto? customer = null;
        if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
        {
            customer = SampleCustomersUiData.FindById(request.CustomerId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            customer = SampleCustomersUiData.FindByMobile(request.CustomerPhone);
        }

        var enrichedRequest = customer is not null
            ? request with
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                CustomerPhone = customer.Mobile,
                CustomerAddress = customer.Address,
                CustomerOutstanding = customer.OutstandingBalance,
                CustomerCreditLimit = customer.CreditLimit,
                CustomerCreditAllowed = customer.CreditAllowed
            }
            : request;

        var existingBefore = !string.IsNullOrWhiteSpace(enrichedRequest.IdempotencyKey)
            ? _billingService.GetAllReceipts().FirstOrDefault(r =>
                r.IdempotencyKey.Equals(enrichedRequest.IdempotencyKey.Trim(), StringComparison.OrdinalIgnoreCase))
            : null;

        var cashierName = User.Identity?.Name ?? "Cashier";
        var settleResult = await _billingService.SettleBillAsync(enrichedRequest, policy, cashierName, cancellationToken);
        if (settleResult.IsFailure)
        {
            return Json(Result.Failure<SettleBillResponseDto>(settleResult.Error));
        }

        var receipt = settleResult.Value;
        var isIdempotentReplay = existingBefore is not null;

        if (!isIdempotentReplay)
        {
            if (receipt.CreditAmount > 0m)
            {
                if (receipt.CustomerId.HasValue && receipt.CustomerId.Value > 0)
                {
                    SampleCustomersUiData.RecordCreditDebtById(receipt.CustomerId.Value, receipt.CreditAmount);
                }
                else if (!string.IsNullOrWhiteSpace(receipt.CustomerPhone))
                {
                    SampleCustomersUiData.RecordCreditDebt(receipt.CustomerPhone, receipt.CreditAmount);
                }
            }

            var tableOrCust = receipt.CreditAmount > 0m
                ? $"{receipt.CustomerName} ({receipt.CustomerPhone})"
                : !string.IsNullOrWhiteSpace(receipt.ParcelToken)
                    ? $"Parcel {receipt.ParcelToken} ({receipt.CustomerName})"
                    : !string.IsNullOrWhiteSpace(receipt.TableCode)
                        ? $"Table {receipt.TableCode}"
                        : receipt.CustomerName;

            SampleBillingUiData.RecordSettledBillWithNumber(
                receipt.BillNumber,
                receipt.OrderType,
                tableOrCust,
                receipt.Totals.GrandTotal,
                receipt.PrimaryPaymentMode);

            await _auditUiData.LogAsync(
                cashierName,
                User.IsInRole("Admin") ? "Admin" : "Cashier",
                "Bill Settled",
                "Billing",
                $"Settled Bill #{receipt.BillNumber} for Rs {receipt.Totals.GrandTotal:0.00} via {receipt.PrimaryPaymentMode} ({receipt.OrderType}). Voucher: {receipt.VoucherNumber}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                "Info",
                cancellationToken);
        }

        var paperSize = settings.GetValueOrDefault("Printing.PaperSize", "80mm");
        var autoPrint = ParseBool(settings.GetValueOrDefault("Printing.AutoPrintOnSettle"), true);
        var copies = int.TryParse(settings.GetValueOrDefault("Printing.Copies"), out var c) && c is >= 1 and <= 5 ? c : 1;
        var hotelName = settings.GetValueOrDefault("HotelNameEn", "Grand Restaurant");

        var receiptUrl = Url.Action("Bill", "Receipt", new { billNo = receipt.BillNumber, paper = paperSize }) ?? $"/Receipt/Bill/{receipt.BillNumber}";
        string? parcelTokenUrl = !string.IsNullOrWhiteSpace(receipt.ParcelToken)
            ? Url.Action("ParcelToken", "Receipt", new { billNo = receipt.BillNumber, paper = paperSize }) ?? $"/Receipt/ParcelToken/{receipt.BillNumber}"
            : null;

        var whatsAppText = $"Namaste! Thank you for dining at {hotelName}. Bill #{receipt.BillNumber} | Total: Rs {receipt.Totals.GrandTotal:0.00} | Mode: {receipt.PrimaryPaymentMode}. Visit again!";

        var response = new SettleBillResponseDto(
            BillNumber: receipt.BillNumber,
            GrandTotal: receipt.Totals.GrandTotal,
            PrimaryPaymentMode: receipt.PrimaryPaymentMode,
            OrderType: receipt.OrderType,
            ParcelToken: receipt.ParcelToken,
            ReceiptUrl: receiptUrl,
            ParcelTokenUrl: parcelTokenUrl,
            AutoPrint: autoPrint,
            PaperSize: paperSize,
            Copies: copies,
            WhatsAppText: whatsAppText,
            VoucherNumber: receipt.VoucherNumber,
            ChangeToReturn: receipt.ChangeToReturn,
            CreditAmount: receipt.CreditAmount,
            NewOutstanding: receipt.NewOutstanding,
            CustomerPhone: receipt.CustomerPhone);

        return Json(Result.Success(response));
    }

    [HttpGet]
    public IActionResult SearchCustomers([FromQuery] string? q)
    {
        var all = SampleCustomersUiData.GetAllCustomers();
        var term = q?.Trim().ToLowerInvariant() ?? string.Empty;

        var matches = string.IsNullOrWhiteSpace(term)
            ? all.Take(15).ToList()
            : all.Where(c =>
                c.Name.ToLowerInvariant().Contains(term) ||
                c.Mobile.Contains(term) ||
                (c.Address != null && c.Address.ToLowerInvariant().Contains(term)))
                .Take(15)
                .ToList();

        return Json(Result.Success(matches));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult QuickAddCustomer([FromBody] QuickAddCustomerRequest? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Name))
        {
            return Json(Result.Failure<CustomerListItemDto>("Customer.NameRequired", "Customer full name is required."));
        }

        var mobile = (request.Mobile ?? string.Empty).Trim();
        if (!MobileRegex.IsMatch(mobile))
        {
            return Json(Result.Failure<CustomerListItemDto>("Customer.InvalidMobile", "Enter a valid 10-digit mobile number starting with 6-9."));
        }

        var address = string.IsNullOrWhiteSpace(request.Address) ? "Local Walk-in" : request.Address.Trim();
        var limit = Math.Max(0m, request.CreditLimit);
        var customer = SampleCustomersUiData.AddCustomer(request.Name.Trim(), mobile, address, limit);
        return Json(Result.Success(customer));
    }

    private async Task<IReadOnlyDictionary<string, string>> GetSafeSettingsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _settingService.GetAllAsync(cancellationToken);
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    private static bool ParseBool(string? value, bool fallback) =>
        bool.TryParse(value, out var parsed) ? parsed : fallback;
}
