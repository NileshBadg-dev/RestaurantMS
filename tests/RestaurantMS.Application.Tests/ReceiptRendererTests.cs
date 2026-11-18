using RestaurantMS.Application.Billing.Services;
using RestaurantMS.Application.Common.Models.Billing;

namespace RestaurantMS.Application.Tests;

public class ReceiptRendererTests
{
    private readonly ReceiptRenderer _renderer = new();

    private static SettledBillReceiptDto BuildSampleReceipt(decimal creditAmount = 0m, string? parcelToken = null)
    {
        var totals = new BillCalculationResult(
            Subtotal: 695m,
            DiscountAmount: 45m,
            PackingCharge: 20m,
            TaxableAmount: 670m,
            GstPercent: 5m,
            CgstAmount: 16.75m,
            SgstAmount: 16.75m,
            TotalTax: 33.50m,
            RawTotal: 703.50m,
            RoundOff: 0.50m,
            GrandTotal: 704m);

        var payments = creditAmount > 0m
            ? new List<BillPaymentLineInputDto>
            {
                new("Cash", 704m - creditAmount, CashReceived: 704m - creditAmount),
                new("Udhari", creditAmount)
            }
            : new List<BillPaymentLineInputDto>
            {
                new("Cash", 704m, CashReceived: 1000m)
            };

        return new SettledBillReceiptDto(
            BillNumber: "B-1001",
            IdempotencyKey: "k-1",
            SettledAtUtc: new DateTime(2026, 11, 18, 9, 0, 0, DateTimeKind.Utc),
            OrderType: parcelToken is null ? "Dine-in" : "Parcel",
            TableCode: parcelToken is null ? "T2" : null,
            ParcelToken: parcelToken,
            WaiterName: "Sachin",
            CashierName: "admin",
            Items:
            [
                new("Paneer Butter Masala", "पनीर बटर मसाला", 2, 280m, 5m),
                new("Butter Naan", "बटर नान", 3, 45m, 5m)
            ],
            Totals: totals,
            PrimaryPaymentMode: creditAmount > 0m ? "Split" : "Cash",
            Payments: payments,
            CashReceivedTotal: creditAmount > 0m ? 704m - creditAmount : 1000m,
            ChangeToReturn: creditAmount > 0m ? 0m : 296m,
            CustomerId: 1,
            CustomerName: "Ramesh Patil",
            CustomerPhone: "9876543210",
            CustomerAddress: "Shivaji Nagar, Pune",
            CreditAmount: creditAmount,
            PreviousOutstanding: creditAmount > 0m ? 1000m : 0m,
            NewOutstanding: creditAmount > 0m ? 1000m + creditAmount : 0m,
            VoucherNumber: "VCH-0001",
            IsDuplicate: false);
    }

    [Fact]
    public void RenderBillReceiptHtml_80mm_ContainsHeaderTotalsTaxBreakupAndChange()
    {
        var receipt = BuildSampleReceipt();
        var options = new ReceiptRenderOptions(
            HotelName: "Grand Restaurant",
            HotelNameMr: "भव्य रेस्टॉरंट",
            Address: "FC Road, Pune",
            Gstin: "27AAAAA0000A1Z5",
            Fssai: "11521036000123",
            PaperSize: "80mm",
            FooterText: "Visit again!",
            HotelUpiId: "grandrestaurant@okhdfc",
            ShowUpiQr: true,
            ShowMarathiNames: true);

        var html = _renderer.RenderBillReceiptHtml(receipt, options);

        Assert.Contains("@page { size: 80mm auto;", html);
        Assert.Contains("Grand Restaurant", html);
        Assert.Contains("भव्य रेस्टॉरंट", html);
        Assert.Contains("GSTIN: 27AAAAA0000A1Z5", html);
        Assert.Contains("FSSAI: 11521036000123", html);
        Assert.Contains("B-1001", html);
        Assert.Contains("Paneer Butter Masala", html);
        Assert.Contains("पनीर बटर मसाला", html);
        Assert.Contains("CGST (2.5%)", html);
        Assert.Contains("Rs 16.75", html);
        Assert.Contains("SGST (2.5%)", html);
        Assert.Contains("GRAND TOTAL", html);
        Assert.Contains("Rs 704.00", html);
        Assert.Contains("Change Returned", html);
        Assert.Contains("Rs 296.00", html);
        Assert.Contains("upi://pay?pa=grandrestaurant%40okhdfc", html);
        Assert.DoesNotContain("DUPLICATE", html);
    }

    [Fact]
    public void RenderBillReceiptHtml_58mmAndA4_GeneratesMatchingPageSizes()
    {
        var receipt = BuildSampleReceipt();

        var html58 = _renderer.RenderBillReceiptHtml(receipt, new ReceiptRenderOptions("Grand Restaurant", PaperSize: "58mm"));
        var htmlA4 = _renderer.RenderBillReceiptHtml(receipt, new ReceiptRenderOptions("Grand Restaurant", PaperSize: "A4"));

        Assert.Contains("@page { size: 58mm auto;", html58);
        Assert.Contains("width: 54mm;", html58);
        Assert.Contains("@page { size: A4 portrait;", htmlA4);
        Assert.Contains("width: 190mm;", htmlA4);
    }

    [Fact]
    public void RenderBillReceiptHtml_DuplicateFlag_IncludesDuplicateWatermark()
    {
        var receipt = BuildSampleReceipt();
        var options = new ReceiptRenderOptions("Grand Restaurant", IsDuplicate: true);

        var html = _renderer.RenderBillReceiptHtml(receipt, options);

        Assert.Contains("DUPLICATE", html);
        Assert.Contains("DUPLICATE COPY", html);
    }

    [Fact]
    public void RenderBillReceiptHtml_WithUdhariCredit_ShowsPreviousCreditAndNewOutstanding()
    {
        var receipt = BuildSampleReceipt(creditAmount: 500m);
        var options = new ReceiptRenderOptions("Grand Restaurant", ShowMarathiNames: false, ShowUpiQr: false);

        var html = _renderer.RenderBillReceiptHtml(receipt, options);

        Assert.Contains("Udhari / Credit Summary", html);
        Assert.Contains("Previous Outstanding", html);
        Assert.Contains("Rs 1000.00", html);
        Assert.Contains("Credit Posted (This Bill)", html);
        Assert.Contains("Rs 500.00", html);
        Assert.Contains("New Outstanding", html);
        Assert.Contains("Rs 1500.00", html);
        Assert.DoesNotContain("पनीर बटर मसाला", html);
        Assert.DoesNotContain("upi://pay", html);
    }

    [Fact]
    public void RenderParcelTokenHtml_ContainsTokenAndItemQuantities()
    {
        var receipt = BuildSampleReceipt(parcelToken: "P-007");
        var html = _renderer.RenderParcelTokenHtml(receipt, new ReceiptRenderOptions("Grand Restaurant", PaperSize: "80mm"));

        Assert.Contains("PARCEL TOKEN SLIP", html);
        Assert.Contains("P-007", html);
        Assert.Contains("Paneer Butter Masala", html);
        Assert.Contains("Total Items: 5", html);
    }

    [Fact]
    public void RenderKotHtml_ContainsKotDetailsAndReprintBanner()
    {
        var kot = new KotPrintDto(
            KotNumber: "KOT-105",
            CreatedAtUtc: new DateTime(2026, 11, 18, 9, 15, 0, DateTimeKind.Utc),
            OrderType: "Dine-in",
            TableOrToken: "Table T4",
            WaiterName: "Sachin",
            Station: "Tandoor",
            Items: [new KotPrintItemDto("Butter Naan", "बटर नान", 4, "Extra crisp")],
            Notes: "Serve hot",
            IsReprint: true);

        var html = _renderer.RenderKotHtml(kot, new ReceiptRenderOptions("Grand Restaurant", ShowMarathiNames: true));

        Assert.Contains("KITCHEN ORDER TICKET (KOT)", html);
        Assert.Contains("KOT-105", html);
        Assert.Contains("Table T4", html);
        Assert.Contains("4x", html);
        Assert.Contains("Butter Naan", html);
        Assert.Contains("बटर नान", html);
        Assert.Contains("Extra crisp", html);
        Assert.Contains("KOT REPRINT", html);
    }
}
