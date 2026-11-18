using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Billing;
using RestaurantMS.Web.SampleData;

namespace RestaurantMS.Web.Controllers;

[Authorize]
[Route("Receipt")]
public class ReceiptController : Controller
{
    private readonly IBillingService _billingService;
    private readonly IReceiptRenderer _receiptRenderer;
    private readonly ISettingService _settingService;
    private readonly IDateTime _dateTime;

    public ReceiptController(
        IBillingService billingService,
        IReceiptRenderer receiptRenderer,
        ISettingService settingService,
        IDateTime dateTime)
    {
        _billingService = billingService;
        _receiptRenderer = receiptRenderer;
        _settingService = settingService;
        _dateTime = dateTime;
    }

    [HttpGet("Bill/{billNo}")]
    public async Task<IActionResult> Bill(
        string billNo,
        [FromQuery] string? paper = null,
        [FromQuery] bool duplicate = false,
        CancellationToken cancellationToken = default)
    {
        var receipt = _billingService.GetReceipt(billNo);
        if (receipt is null)
        {
            var summary = SampleBillingUiData.GetAllSettledBills()
                .FirstOrDefault(b => b.BillNo.Equals(billNo, StringComparison.OrdinalIgnoreCase));

            if (summary is null)
            {
                return NotFound($"Bill {billNo} was not found.");
            }

            receipt = BuildFallbackReceipt(summary);
        }

        var options = await BuildRenderOptionsAsync(paper, duplicate, cancellationToken);
        var html = _receiptRenderer.RenderBillReceiptHtml(receipt, options);
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpGet("ParcelToken/{billNo}")]
    public async Task<IActionResult> ParcelToken(
        string billNo,
        [FromQuery] string? paper = null,
        CancellationToken cancellationToken = default)
    {
        var receipt = _billingService.GetReceipt(billNo);
        if (receipt is null)
        {
            return NotFound($"Parcel receipt {billNo} was not found.");
        }

        var options = await BuildRenderOptionsAsync(paper, false, cancellationToken);
        var html = _receiptRenderer.RenderParcelTokenHtml(receipt, options);
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpGet("Kot/{kotNo}")]
    public async Task<IActionResult> Kot(
        string kotNo,
        [FromQuery] string? paper = null,
        [FromQuery] bool reprint = false,
        CancellationToken cancellationToken = default)
    {
        var ticket = SampleKitchenUiData.FindTicket(kotNo);
        KotPrintDto kotDto;

        if (ticket is not null)
        {
            kotDto = new KotPrintDto(
                KotNumber: ticket.TicketId,
                CreatedAtUtc: _dateTime.UtcNow,
                OrderType: ticket.IsParcel ? "Parcel" : "Dine-in",
                TableOrToken: ticket.TableOrTakeaway,
                WaiterName: User.Identity?.Name ?? "Waiter",
                Station: "Main Kitchen",
                Items: ticket.Items.Select(i => new KotPrintItemDto(i.ItemName, null, i.Quantity, i.Notes)).ToList(),
                IsReprint: reprint);
        }
        else
        {
            kotDto = new KotPrintDto(
                KotNumber: string.IsNullOrWhiteSpace(kotNo) ? "KOT-101" : kotNo,
                CreatedAtUtc: _dateTime.UtcNow,
                OrderType: "Dine-in",
                TableOrToken: "Table T1",
                WaiterName: User.Identity?.Name ?? "Waiter",
                Station: "Main Kitchen",
                Items: [new KotPrintItemDto("Sample Dish", null, 1, "Standard")],
                IsReprint: reprint);
        }

        var options = await BuildRenderOptionsAsync(paper, false, cancellationToken);
        var html = _receiptRenderer.RenderKotHtml(kotDto, options);
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpPost("PreviewDraft")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreviewDraft([FromBody] SettleBillRequest? request, [FromQuery] string? paper = null, CancellationToken cancellationToken = default)
    {
        if (request is null || request.Items is null || request.Items.Count == 0)
        {
            return BadRequest("Add at least one item to preview the bill.");
        }

        var totals = _billingService.CalculateTotals(new BillCalculationRequest(
            request.OrderType,
            request.Items,
            request.DiscountAmount,
            request.PackingCharge,
            request.GstPercent));

        var draft = new SettledBillReceiptDto(
            BillNumber: "DRAFT",
            IdempotencyKey: "draft",
            SettledAtUtc: _dateTime.UtcNow,
            OrderType: string.IsNullOrWhiteSpace(request.OrderType) ? "Dine-in" : request.OrderType,
            TableCode: request.TableCode,
            ParcelToken: request.ParcelToken,
            WaiterName: string.IsNullOrWhiteSpace(request.WaiterName) ? "Counter" : request.WaiterName,
            CashierName: User.Identity?.Name ?? "Cashier",
            Items: request.Items,
            Totals: totals,
            PrimaryPaymentMode: string.IsNullOrWhiteSpace(request.PaymentMode) ? "Cash" : request.PaymentMode,
            Payments: request.Payments?.Count > 0
                ? request.Payments
                : [new BillPaymentLineInputDto(request.PaymentMode ?? "Cash", totals.GrandTotal)],
            CashReceivedTotal: totals.GrandTotal,
            ChangeToReturn: 0m,
            CustomerId: request.CustomerId,
            CustomerName: string.IsNullOrWhiteSpace(request.CustomerName) ? "Walk-in Customer" : request.CustomerName,
            CustomerPhone: request.CustomerPhone,
            CustomerAddress: request.CustomerAddress,
            CreditAmount: 0m,
            PreviousOutstanding: request.CustomerOutstanding,
            NewOutstanding: request.CustomerOutstanding,
            VoucherNumber: null,
            IsDuplicate: false);

        var options = await BuildRenderOptionsAsync(paper, false, cancellationToken);
        var html = _receiptRenderer.RenderBillReceiptHtml(draft, options);
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpGet("UpiQrSvg")]
    public async Task<IActionResult> UpiQrSvg([FromQuery] decimal amount = 0m, [FromQuery] string? note = null, CancellationToken cancellationToken = default)
    {
        var settings = await GetSafeSettingsAsync(cancellationToken);
        var upiId = settings.GetValueOrDefault("Payments.UpiId", "grandrestaurant@okhdfc");
        var hotelName = settings.GetValueOrDefault("HotelNameEn", "Grand Restaurant");
        var uri = _receiptRenderer.BuildUpiPaymentUri(upiId, hotelName, Math.Max(0m, amount), note ?? "Restaurant Bill");
        var svg = BuildQrSvg(uri, 168);
        return Content(svg, "image/svg+xml; charset=utf-8");
    }

    private async Task<ReceiptRenderOptions> BuildRenderOptionsAsync(string? paperOverride, bool isDuplicate, CancellationToken cancellationToken)
    {
        var settings = await GetSafeSettingsAsync(cancellationToken);
        var paper = !string.IsNullOrWhiteSpace(paperOverride)
            ? paperOverride
            : settings.GetValueOrDefault("Printing.PaperSize", "80mm");

        return new ReceiptRenderOptions(
            HotelName: settings.GetValueOrDefault("HotelNameEn", "Grand Restaurant"),
            HotelNameMr: settings.GetValueOrDefault("HotelNameMr", "भव्य रेस्टॉरंट"),
            LogoUrl: settings.GetValueOrDefault("LogoUrl"),
            Address: settings.GetValueOrDefault("Address", "123 FC Road, Pune, Maharashtra"),
            Phone: settings.GetValueOrDefault("ContactNumber", "9822012345"),
            Gstin: settings.GetValueOrDefault("Gstin"),
            Fssai: settings.GetValueOrDefault("Fssai"),
            PaperSize: paper,
            HeaderText: settings.GetValueOrDefault("ReceiptHeader"),
            FooterText: settings.GetValueOrDefault("ReceiptFooter", "Thank you for dining with us! Visit again."),
            HotelUpiId: settings.GetValueOrDefault("Payments.UpiId", "grandrestaurant@okhdfc"),
            ShowUpiQr: ParseBool(settings.GetValueOrDefault("Printing.ShowUpiQrOnReceipt"), true),
            ShowMarathiNames: ParseBool(settings.GetValueOrDefault("Printing.ShowMarathiItemNames"), true),
            IsDuplicate: isDuplicate,
            Copies: int.TryParse(settings.GetValueOrDefault("Printing.Copies"), out var c) && c is >= 1 and <= 5 ? c : 1);
    }

    private SettledBillReceiptDto BuildFallbackReceipt(RestaurantMS.Application.Common.Models.UiData.BillSummaryRowDto summary)
    {
        var totals = new BillCalculationResult(
            summary.Amount, 0m, 0m, summary.Amount, 0m, 0m, 0m, 0m, summary.Amount, 0m, summary.Amount);

        return new SettledBillReceiptDto(
            BillNumber: summary.BillNo,
            IdempotencyKey: summary.BillNo,
            SettledAtUtc: _dateTime.UtcNow,
            OrderType: summary.OrderType,
            TableCode: summary.TableOrCustomer,
            ParcelToken: null,
            WaiterName: "Counter",
            CashierName: User.Identity?.Name ?? "Cashier",
            Items: [new BillLineInputDto("Order Items", null, 1, summary.Amount, 0m)],
            Totals: totals,
            PrimaryPaymentMode: summary.PaymentMode,
            Payments: [new BillPaymentLineInputDto(summary.PaymentMode, summary.Amount)],
            CashReceivedTotal: summary.PaymentMode.Equals("Cash", StringComparison.OrdinalIgnoreCase) ? summary.Amount : 0m,
            ChangeToReturn: 0m,
            CustomerId: null,
            CustomerName: summary.TableOrCustomer,
            CustomerPhone: null,
            CustomerAddress: null,
            CreditAmount: summary.PaymentMode.Equals("Udhari", StringComparison.OrdinalIgnoreCase) || summary.PaymentMode.Equals("Credit", StringComparison.OrdinalIgnoreCase) ? summary.Amount : 0m,
            PreviousOutstanding: 0m,
            NewOutstanding: 0m,
            VoucherNumber: null,
            IsDuplicate: false);
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

    private static string BuildQrSvg(string payload, int sizePx)
    {
        const int modules = 21;
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        var sb = new StringBuilder(1024);
        sb.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{sizePx}\" height=\"{sizePx}\" viewBox=\"0 0 {modules} {modules}\" data-upi-uri=\"{WebUtility.HtmlEncode(payload)}\">");
        sb.Append($"<rect width=\"{modules}\" height=\"{modules}\" fill=\"#ffffff\"/>");

        void DrawFinder(int ox, int oy)
        {
            sb.Append($"<rect x=\"{ox}\" y=\"{oy}\" width=\"7\" height=\"7\" fill=\"#1B1033\"/>");
            sb.Append($"<rect x=\"{ox + 1}\" y=\"{oy + 1}\" width=\"5\" height=\"5\" fill=\"#ffffff\"/>");
            sb.Append($"<rect x=\"{ox + 2}\" y=\"{oy + 2}\" width=\"3\" height=\"3\" fill=\"#1B1033\"/>");
        }

        DrawFinder(0, 0);
        DrawFinder(modules - 7, 0);
        DrawFinder(0, modules - 7);

        var bitIndex = 0;
        for (var y = 0; y < modules; y++)
        {
            for (var x = 0; x < modules; x++)
            {
                if ((x < 8 && y < 8) || (x >= modules - 8 && y < 8) || (x < 8 && y >= modules - 8))
                {
                    continue;
                }

                var b = hash[(bitIndex / 8) % hash.Length];
                var on = ((b >> (bitIndex % 8)) & 1) == 1;
                bitIndex++;
                if (on)
                {
                    sb.Append($"<rect x=\"{x}\" y=\"{y}\" width=\"1\" height=\"1\" fill=\"#1B1033\"/>");
                }
            }
        }

        sb.Append("</svg>");
        return sb.ToString();
    }
}
