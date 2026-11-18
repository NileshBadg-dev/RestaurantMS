using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Billing;

namespace RestaurantMS.Application.Billing.Services;

public class ReceiptRenderer : IReceiptRenderer
{
    public string BuildUpiPaymentUri(string upiId, string payeeName, decimal amount, string? transactionNote = null)
    {
        var pa = Uri.EscapeDataString((upiId ?? string.Empty).Trim());
        var pn = Uri.EscapeDataString((payeeName ?? "Restaurant").Trim());
        var am = amount.ToString("0.00", CultureInfo.InvariantCulture);
        var tn = Uri.EscapeDataString(string.IsNullOrWhiteSpace(transactionNote) ? "Restaurant Bill" : transactionNote.Trim());
        return $"upi://pay?pa={pa}&pn={pn}&am={am}&cu=INR&tn={tn}";
    }

    public string RenderBillReceiptHtml(SettledBillReceiptDto receipt, ReceiptRenderOptions options)
    {
        var paper = NormalizePaperSize(options.PaperSize);
        var widthCss = GetPaperWidthCss(paper);
        var pageSizeCss = GetPageSizeCss(paper);
        var isA4 = paper == "A4";
        var isDuplicate = options.IsDuplicate || receipt.IsDuplicate;
        var istTime = ToIst(receipt.SettledAtUtc);

        var sb = new StringBuilder(4096);
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\" />");
        sb.AppendLine($"<title>Receipt {Encode(receipt.BillNumber)}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(BuildSharedReceiptCss(widthCss, pageSizeCss, isA4));
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"receipt-sheet\">");

        if (isDuplicate)
        {
            sb.AppendLine("<div class=\"duplicate-watermark\">DUPLICATE</div>");
            sb.AppendLine("<div class=\"duplicate-banner\">*** DUPLICATE COPY ***</div>");
        }

        // Header: logo, hotel name, address, GSTIN, FSSAI
        sb.AppendLine("<div class=\"header text-center\">");
        if (!string.IsNullOrWhiteSpace(options.LogoUrl))
        {
            sb.AppendLine($"<img src=\"{Encode(options.LogoUrl)}\" alt=\"Logo\" class=\"hotel-logo\" onerror=\"this.style.display='none'\" />");
        }

        sb.AppendLine($"<div class=\"hotel-name\">{Encode(options.HotelName)}</div>");
        if (options.ShowMarathiNames && !string.IsNullOrWhiteSpace(options.HotelNameMr))
        {
            sb.AppendLine($"<div class=\"hotel-name-mr\">{Encode(options.HotelNameMr)}</div>");
        }

        if (!string.IsNullOrWhiteSpace(options.Address))
        {
            sb.AppendLine($"<div class=\"meta-line\">{Encode(options.Address)}</div>");
        }

        if (!string.IsNullOrWhiteSpace(options.Phone))
        {
            sb.AppendLine($"<div class=\"meta-line\">Tel: {Encode(options.Phone)}</div>");
        }

        if (!string.IsNullOrWhiteSpace(options.Gstin))
        {
            sb.AppendLine($"<div class=\"meta-line\">GSTIN: {Encode(options.Gstin)}</div>");
        }

        if (!string.IsNullOrWhiteSpace(options.Fssai))
        {
            sb.AppendLine($"<div class=\"meta-line\">FSSAI: {Encode(options.Fssai)}</div>");
        }

        if (!string.IsNullOrWhiteSpace(options.HeaderText))
        {
            sb.AppendLine($"<div class=\"custom-header\">{Encode(options.HeaderText)}</div>");
        }

        sb.AppendLine("</div>");
        sb.AppendLine("<div class=\"divider\"></div>");

        // Bill metadata
        sb.AppendLine("<div class=\"meta-grid\">");
        sb.AppendLine($"<div><strong>Bill No:</strong> {Encode(receipt.BillNumber)}</div>");
        sb.AppendLine($"<div class=\"text-right\"><strong>Date:</strong> {istTime:dd-MMM-yyyy hh:mm tt}</div>");

        if (!string.IsNullOrWhiteSpace(receipt.ParcelToken))
        {
            sb.AppendLine($"<div><strong>Parcel Token:</strong> {Encode(receipt.ParcelToken)}</div>");
        }
        else if (!string.IsNullOrWhiteSpace(receipt.TableCode))
        {
            sb.AppendLine($"<div><strong>Table:</strong> {Encode(receipt.TableCode)} ({Encode(receipt.OrderType)})</div>");
        }
        else
        {
            sb.AppendLine($"<div><strong>Type:</strong> {Encode(receipt.OrderType)}</div>");
        }

        sb.AppendLine($"<div class=\"text-right\"><strong>Waiter:</strong> {Encode(receipt.WaiterName)}</div>");
        sb.AppendLine($"<div><strong>Cashier:</strong> {Encode(receipt.CashierName)}</div>");
        sb.AppendLine($"<div class=\"text-right\"><strong>Customer:</strong> {Encode(receipt.CustomerName)}</div>");
        sb.AppendLine("</div>");

        sb.AppendLine("<div class=\"divider\"></div>");

        // Items Table
        sb.AppendLine("<table class=\"items-table\">");
        sb.AppendLine("<thead><tr><th class=\"text-left\">Item</th><th class=\"text-center\">Qty</th><th class=\"text-right\">Rate</th><th class=\"text-right\">Amt</th></tr></thead>");
        sb.AppendLine("<tbody>");
        foreach (var item in receipt.Items)
        {
            var amount = Math.Round(item.Quantity * item.Rate, 2, MidpointRounding.AwayFromZero);
            sb.AppendLine("<tr>");
            sb.Append("<td class=\"text-left\">");
            sb.Append($"<div class=\"item-name\">{Encode(item.ItemName)}</div>");
            if (options.ShowMarathiNames && !string.IsNullOrWhiteSpace(item.MarathiName))
            {
                sb.Append($"<div class=\"item-mr\">{Encode(item.MarathiName)}</div>");
            }
            sb.AppendLine("</td>");
            sb.AppendLine($"<td class=\"text-center\">{item.Quantity}</td>");
            sb.AppendLine($"<td class=\"text-right\">{item.Rate:0.00}</td>");
            sb.AppendLine($"<td class=\"text-right\">{amount:0.00}</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");

        sb.AppendLine("<div class=\"divider\"></div>");

        // Totals & GST Breakup
        var t = receipt.Totals;
        sb.AppendLine("<table class=\"totals-table\">");
        sb.AppendLine($"<tr><td>Subtotal</td><td class=\"text-right\">Rs {t.Subtotal:0.00}</td></tr>");
        if (t.DiscountAmount > 0m)
        {
            sb.AppendLine($"<tr><td>Discount</td><td class=\"text-right\">- Rs {t.DiscountAmount:0.00}</td></tr>");
        }

        if (t.PackingCharge > 0m)
        {
            sb.AppendLine($"<tr><td>Packing Charge</td><td class=\"text-right\">Rs {t.PackingCharge:0.00}</td></tr>");
        }

        if (t.TotalTax > 0m)
        {
            var halfRate = Math.Round(t.GstPercent / 2m, 2);
            sb.AppendLine($"<tr><td>CGST ({halfRate:0.##}%)</td><td class=\"text-right\">Rs {t.CgstAmount:0.00}</td></tr>");
            sb.AppendLine($"<tr><td>SGST ({halfRate:0.##}%)</td><td class=\"text-right\">Rs {t.SgstAmount:0.00}</td></tr>");
        }

        if (t.RoundOff != 0m)
        {
            var sign = t.RoundOff > 0m ? "+" : "-";
            sb.AppendLine($"<tr><td>Round Off</td><td class=\"text-right\">{sign} Rs {Math.Abs(t.RoundOff):0.00}</td></tr>");
        }

        sb.AppendLine($"<tr class=\"grand-total-row\"><td>GRAND TOTAL</td><td class=\"text-right\">Rs {t.GrandTotal:0.00}</td></tr>");
        sb.AppendLine("</table>");

        sb.AppendLine("<div class=\"divider\"></div>");

        // Payments Section
        sb.AppendLine("<div class=\"section-title\">Payment Details</div>");
        sb.AppendLine("<table class=\"totals-table\">");
        foreach (var p in receipt.Payments)
        {
            var details = BuildPaymentLineLabel(p);
            sb.AppendLine($"<tr><td>{Encode(details)}</td><td class=\"text-right\">Rs {p.Amount:0.00}</td></tr>");
        }

        if (receipt.CashReceivedTotal > 0m && receipt.ChangeToReturn > 0m)
        {
            sb.AppendLine($"<tr><td>Cash Received</td><td class=\"text-right\">Rs {receipt.CashReceivedTotal:0.00}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Change Returned</strong></td><td class=\"text-right\"><strong>Rs {receipt.ChangeToReturn:0.00}</strong></td></tr>");
        }

        sb.AppendLine("</table>");

        // Credit / Udhari block
        if (receipt.CreditAmount > 0m)
        {
            sb.AppendLine("<div class=\"credit-box\">");
            sb.AppendLine("<div class=\"section-title\">Udhari / Credit Summary</div>");
            sb.AppendLine($"<div>Customer: <strong>{Encode(receipt.CustomerName)}</strong>{(!string.IsNullOrWhiteSpace(receipt.CustomerPhone) ? $" ({Encode(receipt.CustomerPhone)})" : string.Empty)}</div>");
            sb.AppendLine("<table class=\"totals-table\">");
            sb.AppendLine($"<tr><td>Previous Outstanding</td><td class=\"text-right\">Rs {receipt.PreviousOutstanding:0.00}</td></tr>");
            sb.AppendLine($"<tr><td>Credit Posted (This Bill)</td><td class=\"text-right\">Rs {receipt.CreditAmount:0.00}</td></tr>");
            sb.AppendLine($"<tr><td><strong>New Outstanding</strong></td><td class=\"text-right\"><strong>Rs {receipt.NewOutstanding:0.00}</strong></td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
        }

        // Optional UPI QR
        if (options.ShowUpiQr && !string.IsNullOrWhiteSpace(options.HotelUpiId))
        {
            var upiUri = BuildUpiPaymentUri(options.HotelUpiId, options.HotelName, t.GrandTotal, $"Bill {receipt.BillNumber}");
            sb.AppendLine("<div class=\"qr-block text-center\">");
            sb.AppendLine(BuildDeterministicQrSvg(upiUri, 96));
            sb.AppendLine($"<div class=\"qr-caption\">Scan &amp; Pay via UPI: {Encode(options.HotelUpiId)}</div>");
            sb.AppendLine("</div>");
        }

        // Footer
        sb.AppendLine("<div class=\"divider\"></div>");
        sb.AppendLine("<div class=\"footer text-center\">");
        var footer = string.IsNullOrWhiteSpace(options.FooterText)
            ? "Thank you for dining with us! Visit again."
            : options.FooterText;
        sb.AppendLine($"<div>{Encode(footer)}</div>");
        if (!string.IsNullOrWhiteSpace(receipt.VoucherNumber))
        {
            sb.AppendLine($"<div class=\"meta-line\">Ref Voucher: {Encode(receipt.VoucherNumber)}</div>");
        }
        sb.AppendLine("</div>");

        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    public string RenderParcelTokenHtml(SettledBillReceiptDto receipt, ReceiptRenderOptions options)
    {
        var paper = NormalizePaperSize(options.PaperSize);
        var widthCss = GetPaperWidthCss(paper == "A4" ? "80mm" : paper);
        var pageSizeCss = GetPageSizeCss(paper == "A4" ? "80mm" : paper);
        var istTime = ToIst(receipt.SettledAtUtc);
        var token = string.IsNullOrWhiteSpace(receipt.ParcelToken) ? receipt.BillNumber : receipt.ParcelToken;

        var sb = new StringBuilder(2048);
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\" />");
        sb.AppendLine($"<title>Parcel Token {Encode(token)}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(BuildSharedReceiptCss(widthCss, pageSizeCss, false));
        sb.AppendLine(".token-big { font-size: 28px; font-weight: 800; letter-spacing: 2px; border: 2px solid #000; padding: 6px 10px; margin: 8px 0; display: inline-block; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"receipt-sheet text-center\">");
        sb.AppendLine($"<div class=\"hotel-name\">{Encode(options.HotelName)}</div>");
        sb.AppendLine("<div class=\"section-title\">PARCEL TOKEN SLIP</div>");
        sb.AppendLine($"<div class=\"token-big\">{Encode(token)}</div>");
        sb.AppendLine($"<div class=\"meta-line\">Bill No: {Encode(receipt.BillNumber)} | {istTime:dd-MMM-yyyy hh:mm tt}</div>");
        sb.AppendLine($"<div class=\"meta-line\">Customer: {Encode(receipt.CustomerName)}</div>");
        sb.AppendLine("<div class=\"divider\"></div>");
        sb.AppendLine("<table class=\"items-table\">");
        sb.AppendLine("<thead><tr><th class=\"text-left\">Item</th><th class=\"text-right\">Qty</th></tr></thead><tbody>");
        foreach (var item in receipt.Items)
        {
            sb.AppendLine($"<tr><td class=\"text-left\">{Encode(item.ItemName)}</td><td class=\"text-right\"><strong>{item.Quantity}</strong></td></tr>");
        }
        sb.AppendLine("</tbody></table>");
        sb.AppendLine("<div class=\"divider\"></div>");
        sb.AppendLine($"<div class=\"meta-line\"><strong>Total Items: {receipt.Items.Sum(i => i.Quantity)} | Paid: Rs {receipt.Totals.GrandTotal:0.00}</strong></div>");
        sb.AppendLine("<div class=\"footer\">Please present this token at the parcel pickup counter.</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    public string RenderKotHtml(KotPrintDto kot, ReceiptRenderOptions options)
    {
        var paper = NormalizePaperSize(options.PaperSize);
        var widthCss = GetPaperWidthCss(paper == "A4" ? "80mm" : paper);
        var pageSizeCss = GetPageSizeCss(paper == "A4" ? "80mm" : paper);
        var istTime = ToIst(kot.CreatedAtUtc);

        var sb = new StringBuilder(2048);
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\" />");
        sb.AppendLine($"<title>KOT {Encode(kot.KotNumber)}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(BuildSharedReceiptCss(widthCss, pageSizeCss, false));
        sb.AppendLine(".kot-title { font-size: 16px; font-weight: 800; border: 1.5px solid #000; padding: 4px; margin: 4px 0; }");
        sb.AppendLine(".kot-qty { font-size: 15px; font-weight: 800; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"receipt-sheet\">");

        if (kot.IsReprint)
        {
            sb.AppendLine("<div class=\"duplicate-banner\">*** KOT REPRINT ***</div>");
        }

        sb.AppendLine("<div class=\"text-center\">");
        sb.AppendLine($"<div class=\"hotel-name\">{Encode(options.HotelName)}</div>");
        sb.AppendLine($"<div class=\"kot-title\">KITCHEN ORDER TICKET (KOT)</div>");
        sb.AppendLine($"<div><strong>{Encode(kot.KotNumber)}</strong> &bull; {Encode(kot.Station)}</div>");
        sb.AppendLine("</div>");

        sb.AppendLine("<div class=\"divider\"></div>");
        sb.AppendLine("<div class=\"meta-grid\">");
        sb.AppendLine($"<div><strong>Table/Token:</strong> {Encode(kot.TableOrToken)}</div>");
        sb.AppendLine($"<div class=\"text-right\"><strong>Type:</strong> {Encode(kot.OrderType)}</div>");
        sb.AppendLine($"<div><strong>Waiter:</strong> {Encode(kot.WaiterName)}</div>");
        sb.AppendLine($"<div class=\"text-right\">{istTime:dd-MMM hh:mm tt}</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("<div class=\"divider\"></div>");

        sb.AppendLine("<table class=\"items-table\">");
        sb.AppendLine("<thead><tr><th class=\"text-left\">Qty</th><th class=\"text-left\">Dish</th></tr></thead><tbody>");
        foreach (var item in kot.Items)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td class=\"kot-qty\">{item.Quantity}x</td>");
            sb.Append("<td>");
            sb.Append($"<div class=\"item-name\">{Encode(item.ItemName)}</div>");
            if (options.ShowMarathiNames && !string.IsNullOrWhiteSpace(item.MarathiName))
            {
                sb.Append($"<div class=\"item-mr\">{Encode(item.MarathiName)}</div>");
            }
            if (!string.IsNullOrWhiteSpace(item.Notes))
            {
                sb.Append($"<div class=\"meta-line\"><em>Note: {Encode(item.Notes)}</em></div>");
            }
            sb.AppendLine("</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody></table>");

        if (!string.IsNullOrWhiteSpace(kot.Notes))
        {
            sb.AppendLine("<div class=\"divider\"></div>");
            sb.AppendLine($"<div class=\"meta-line\"><strong>Kitchen Note:</strong> {Encode(kot.Notes)}</div>");
        }

        sb.AppendLine("<div class=\"divider\"></div>");
        sb.AppendLine($"<div class=\"text-center meta-line\">Total Qty: {kot.Items.Sum(i => i.Quantity)}</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    private static string BuildPaymentLineLabel(BillPaymentLineInputDto p)
    {
        if (p.Mode.Equals("UPI", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(p.UpiReference) ? "UPI" : $"UPI (UTR: {p.UpiReference.Trim()})";
        }

        if (p.Mode.Equals("Card", StringComparison.OrdinalIgnoreCase))
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(p.CardLast4))
            {
                parts.Add($"xxxx {p.CardLast4.Trim()}");
            }
            if (!string.IsNullOrWhiteSpace(p.CardApprovalCode))
            {
                parts.Add($"Auth: {p.CardApprovalCode.Trim()}");
            }
            if (!string.IsNullOrWhiteSpace(p.CardBankName))
            {
                parts.Add(p.CardBankName.Trim());
            }
            return parts.Count > 0 ? $"Card ({string.Join(", ", parts)})" : "Card";
        }

        if (p.Mode.Equals("Udhari", StringComparison.OrdinalIgnoreCase) || p.Mode.Equals("Credit", StringComparison.OrdinalIgnoreCase))
        {
            return "Udhari (Credit)";
        }

        return "Cash";
    }

    private static string NormalizePaperSize(string? paperSize)
    {
        if (string.IsNullOrWhiteSpace(paperSize))
        {
            return "80mm";
        }

        var normalized = paperSize.Trim().ToUpperInvariant();
        return normalized switch
        {
            "58MM" => "58mm",
            "A4" => "A4",
            _ => "80mm"
        };
    }

    private static string GetPaperWidthCss(string paper) => paper switch
    {
        "58mm" => "54mm",
        "A4" => "190mm",
        _ => "74mm"
    };

    private static string GetPageSizeCss(string paper) => paper switch
    {
        "58mm" => "58mm auto",
        "A4" => "A4 portrait",
        _ => "80mm auto"
    };

    private static string BuildSharedReceiptCss(string widthCss, string pageSizeCss, bool isA4)
    {
        var baseFontSize = isA4 ? "13px" : widthCss == "54mm" ? "10px" : "11.5px";
        return $@"
@page {{ size: {pageSizeCss}; margin: {(isA4 ? "10mm" : "2mm")}; }}
* {{ box-sizing: border-box; }}
body {{
    margin: 0;
    padding: {(isA4 ? "12px" : "4px")};
    background: #ffffff;
    color: #111111;
    font-family: 'Courier New', 'Noto Sans Devanagari', monospace;
    font-size: {baseFontSize};
    line-height: 1.35;
}}
.receipt-sheet {{
    position: relative;
    width: {widthCss};
    margin: 0 auto;
    padding: {(isA4 ? "16px 20px" : "4px")};
    border: {(isA4 ? "1px solid #d1d5db" : "none")};
}}
.text-center {{ text-align: center; }}
.text-left {{ text-align: left; }}
.text-right {{ text-align: right; }}
.hotel-logo {{ max-height: 44px; max-width: 120px; margin-bottom: 4px; }}
.hotel-name {{ font-size: {(isA4 ? "20px" : "14px")}; font-weight: 800; text-transform: uppercase; }}
.hotel-name-mr {{ font-size: {(isA4 ? "15px" : "12px")}; font-weight: 700; margin-bottom: 2px; }}
.meta-line {{ font-size: 0.9em; color: #222; }}
.custom-header {{ margin-top: 4px; font-style: italic; }}
.divider {{ border-top: 1px dashed #222; margin: 6px 0; }}
.meta-grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 2px 6px; font-size: 0.92em; }}
.items-table, .totals-table {{ width: 100%; border-collapse: collapse; }}
.items-table th {{ border-bottom: 1px solid #222; padding: 3px 2px; font-weight: 700; }}
.items-table td {{ padding: 3px 2px; vertical-align: top; }}
.item-name {{ font-weight: 600; }}
.item-mr {{ font-size: 0.85em; color: #333; }}
.totals-table td {{ padding: 2px 0; }}
.grand-total-row td {{ font-size: 1.15em; font-weight: 800; border-top: 1px solid #222; border-bottom: 1px solid #222; padding: 4px 0; }}
.section-title {{ font-weight: 700; text-transform: uppercase; font-size: 0.92em; margin-bottom: 3px; }}
.credit-box {{ border: 1px dashed #111; padding: 5px; margin-top: 6px; }}
.qr-block {{ margin-top: 8px; }}
.qr-caption {{ font-size: 0.85em; margin-top: 3px; }}
.footer {{ margin-top: 6px; font-size: 0.9em; }}
.duplicate-banner {{ text-align: center; font-weight: 800; border: 1px solid #000; padding: 2px; margin-bottom: 6px; letter-spacing: 1px; }}
.duplicate-watermark {{
    position: fixed;
    top: 40%;
    left: 50%;
    transform: translate(-50%, -50%) rotate(-32deg);
    font-size: {(isA4 ? "64px" : "28px")};
    font-weight: 900;
    color: rgba(0, 0, 0, 0.10);
    pointer-events: none;
    letter-spacing: 4px;
    z-index: 10;
}}
@media print {{
    body {{ padding: 0; }}
    .receipt-sheet {{ border: none; }}
}}";
    }

    private static string BuildDeterministicQrSvg(string payload, int sizePx)
    {
        const int modules = 21;
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        var sb = new StringBuilder(1024);
        sb.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{sizePx}\" height=\"{sizePx}\" viewBox=\"0 0 {modules} {modules}\" data-upi-uri=\"{Encode(payload)}\" aria-label=\"UPI QR Code\">");
        sb.Append($"<rect width=\"{modules}\" height=\"{modules}\" fill=\"#ffffff\"/>");

        void DrawFinder(int ox, int oy)
        {
            sb.Append($"<rect x=\"{ox}\" y=\"{oy}\" width=\"7\" height=\"7\" fill=\"#000000\"/>");
            sb.Append($"<rect x=\"{ox + 1}\" y=\"{oy + 1}\" width=\"5\" height=\"5\" fill=\"#ffffff\"/>");
            sb.Append($"<rect x=\"{ox + 2}\" y=\"{oy + 2}\" width=\"3\" height=\"3\" fill=\"#000000\"/>");
        }

        DrawFinder(0, 0);
        DrawFinder(modules - 7, 0);
        DrawFinder(0, modules - 7);

        var bitIndex = 0;
        for (var y = 0; y < modules; y++)
        {
            for (var x = 0; x < modules; x++)
            {
                var inTopLeft = x < 8 && y < 8;
                var inTopRight = x >= modules - 8 && y < 8;
                var inBottomLeft = x < 8 && y >= modules - 8;
                if (inTopLeft || inTopRight || inBottomLeft)
                {
                    continue;
                }

                var b = hash[(bitIndex / 8) % hash.Length];
                var on = ((b >> (bitIndex % 8)) & 1) == 1;
                bitIndex++;
                if (on)
                {
                    sb.Append($"<rect x=\"{x}\" y=\"{y}\" width=\"1\" height=\"1\" fill=\"#000000\"/>");
                }
            }
        }

        sb.Append("</svg>");
        return sb.ToString();
    }

    private static DateTime ToIst(DateTime utc)
    {
        var normalized = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return normalized.AddHours(5).AddMinutes(30);
    }

    private static string Encode(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);
}
