using System.Globalization;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Reports;
using RestaurantMS.Application.Inventory.Services;

namespace RestaurantMS.Application.Reports.Services;

/// <summary>
/// Generates filterable datasets, KPI cards, charts and drill-down metadata for all 26 reports.
/// </summary>
public class ReportDataGenerator
{
    private readonly IBillingService _billingService;
    private readonly IAccountingService _accountingService;
    private readonly InventoryStateStore _inventoryStore;
    private readonly IDateTime _dateTime;

    public ReportDataGenerator(
        IBillingService billingService,
        IAccountingService accountingService,
        InventoryStateStore inventoryStore,
        IDateTime dateTime)
    {
        _billingService = billingService;
        _accountingService = accountingService;
        _inventoryStore = inventoryStore;
        _dateTime = dateTime;
    }

    public GeneratedReportPayload Build(ReportDefinitionDto def, ReportFilterModel filter)
    {
        return def.Code.ToUpperInvariant() switch
        {
            "RPT01" => BuildSalesSummary(filter),
            "RPT02" => BuildGstTaxReport(filter),
            "RPT03" => BuildCategorySales(filter),
            "RPT04" => BuildWaiterCashierSales(filter),
            "RPT05" => BuildRevenueTrend(filter),
            "RPT06" => BuildPaymentModeSplit(filter),
            "RPT07" => BuildBillRegister(filter),
            "RPT08" => BuildExpensesReport(filter),
            "RPT09" => BuildStockReport(filter),
            "RPT10" => BuildPurchasesAndMovementsReport(filter),
            "RPT11" => BuildCreditRecoveryReport(filter),
            "RPT12" => BuildTableSpentTimeReport(filter),
            "RPT13" => BuildKitchenPerformanceReport(filter),
            "RPT14" => BuildBillDeliveryLog(filter),
            "RPT15" => BuildDiscountsReport(filter),
            "RPT16" => BuildHourlySalesReport(filter),
            "RPT17" => BuildCancelledBillsReport(filter),
            "RPT18" => BuildDayClosingReport(filter),
            "RPT19" => BuildTableCollectionReport(filter),
            "RPT20" => BuildMenuCardReport(filter),
            "RPT21" => BuildItemWiseSalesReport(filter),
            "RPT22" => BuildParcelReport(filter),
            "RPT23" => BuildAttendanceReport(filter),
            "RPT24" => BuildEmployeeAdvancesReport(filter),
            "RPT25" => BuildCustomerAgeingReport(filter),
            "RPT26" => BuildAccountsSummaryReport(filter),
            _ => BuildSalesSummary(filter)
        };
    }

    private GeneratedReportPayload BuildSalesSummary(ReportFilterModel filter)
    {
        var liveReceipts = _billingService.GetAllReceipts();
        var liveTotal = liveReceipts.Sum(r => r.Totals.GrandTotal);
        var rows = new List<ReportRowSeed>
        {
            Row(["Dine-in", "Morning", "112", "₹1,42,800.00", "₹3,200.00", "₹6,980.00", "₹1,46,580.00"], "Dine-in", "Dine-in", "Morning", "All", "All", "All", "Cash", "Dine-in", "BILL-SUM-1", "Dine-in Morning Summary", 146580m),
            Row(["Dine-in", "Evening", "184", "₹2,86,400.00", "₹6,800.00", "₹13,980.00", "₹2,93,580.00"], "Dine-in", "Dine-in", "Evening", "All", "All", "All", "UPI", "Dine-in", "BILL-SUM-2", "Dine-in Evening Summary", 293580m),
            Row(["Parcel", "Morning", "64", "₹54,200.00", "₹900.00", "₹2,665.00", "₹55,965.00"], "Parcel", "Parcel", "Morning", "All", "All", "All", "UPI", "Parcel", "BILL-SUM-3", "Parcel Morning Summary", 55965m),
            Row(["Parcel", "Evening", "98", "₹96,400.00", "₹1,600.00", "₹4,740.00", "₹99,540.00"], "Parcel", "Parcel", "Evening", "All", "All", "All", "Cash", "Parcel", "BILL-SUM-4", "Parcel Evening Summary", 99540m),
            Row(["Delivery", "Evening", "72", "₹84,500.00", "₹2,100.00", "₹4,120.00", "₹86,520.00"], "Delivery", "Delivery", "Evening", "All", "All", "All", "Card", "Delivery", "BILL-SUM-5", "Delivery Evening Summary", 86520m)
        };

        var filtered = ApplyFilters(rows, filter);
        var totalRev = filtered.Sum(r => r.NumericMetric) + liveTotal;
        var kpis = new List<ReportKpiCardDto>
        {
            new("Gross Revenue", FormatRs(totalRev), $"{filtered.Count} channel-shift blocks (+{liveReceipts.Count} live bills)", "plum", "bi-currency-rupee"),
            new("Net Taxable Sales", FormatRs(totalRev / 1.05m), "Before 5% GST", "teal", "bi-receipt"),
            new("GST Collected", FormatRs(totalRev - (totalRev / 1.05m)), "CGST 2.5% + SGST 2.5%", "saffron", "bi-percent"),
            new("Estimated Net Margin", "18.4%", "After COGS & overheads", "teal", "bi-graph-up-arrow")
        };

        var chart = new ReportChartDto("bar", "Revenue by Channel & Shift (₹)", filtered.Select(r => $"{r.Cells[0]} ({r.Cells[1]})").ToList(), "Net Revenue (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "", "", "", FormatRs(filtered.Sum(r => r.NumericMetric)) };
        return new(["Channel", "Shift", "Bills", "Gross Sales", "Discount", "GST (5%)", "Net Total"], filtered, totals, kpis, chart, "Channel");
    }

    private GeneratedReportPayload BuildBillRegister(ReportFilterModel filter)
    {
        var waiters = new[] { "Suresh Patil", "Anil Deshmukh", "Meena Jadhav", "Sachin Kale" };
        var tables = new[] { "T1", "T2", "T3", "T4", "T5", "T7", "T9", "Counter" };
        var modes = new[] { "Cash", "UPI", "Card", "Udhari", "Split" };
        var orderTypes = new[] { "Dine-in", "Dine-in", "Parcel", "Delivery" };
        var shifts = new[] { "Morning", "Evening" };
        var categories = new[] { "Main Course", "Biryani & Rice", "Starters", "Breads", "Beverages" };

        var seeds = new List<ReportRowSeed>();
        var today = _dateTime.UtcNow.AddHours(5).AddMinutes(30).Date;

        // Include live settled bills from BillingService first!
        foreach (var live in _billingService.GetAllReceipts())
        {
            var dt = live.SettledAtUtc.AddHours(5).AddMinutes(30);
            seeds.Add(Row(
                [live.BillNumber, dt.ToString("dd-MMM HH:mm"), live.OrderType, live.TableCode ?? live.ParcelToken ?? "Counter", live.CustomerName, live.WaiterName, live.PrimaryPaymentMode, FormatRs(live.Totals.GrandTotal)],
                live.OrderType, "bill", dt.Hour < 16 ? "Morning" : "Evening", live.WaiterName, live.TableCode ?? "Counter", "Main Course", live.PrimaryPaymentMode, live.OrderType,
                live.BillNumber, $"Bill {live.BillNumber} — {live.CustomerName}", live.Totals.GrandTotal,
                new Dictionary<string, string>
                {
                    ["Bill Number"] = live.BillNumber,
                    ["Customer"] = live.CustomerName,
                    ["Order Type"] = live.OrderType,
                    ["Payment Mode"] = live.PrimaryPaymentMode,
                    ["Subtotal"] = FormatRs(live.Totals.Subtotal),
                    ["GST"] = FormatRs(live.Totals.TotalTax),
                    ["Grand Total"] = FormatRs(live.Totals.GrandTotal),
                    ["Voucher"] = live.VoucherNumber ?? "-",
                    ["Items"] = string.Join(", ", live.Items.Select(i => $"{i.Quantity}x {i.ItemName}"))
                }));
        }

        // Generate 60 historical bills so server-side paging (Page 1, 2, 3...) is immediately testable
        for (var i = 1; i <= 60; i++)
        {
            var billNo = $"TAX-{1000 + i}";
            var oType = orderTypes[i % orderTypes.Length];
            var tbl = oType == "Dine-in" ? tables[i % 7] : $"P-{i:D3}";
            var waiter = waiters[i % waiters.Length];
            var mode = modes[i % modes.Length];
            var shift = shifts[i % shifts.Length];
            var cat = categories[i % categories.Length];
            var amt = 260m + (i * 37m % 1450m);
            var billDate = today.AddDays(-(i % 14));
            var cust = i % 3 == 0 ? "Ramesh Patil" : i % 3 == 1 ? "Pooja Verma" : "Walk-in Guest";

            seeds.Add(Row(
                [billNo, billDate.ToString("dd-MMM-yyyy"), oType, tbl, cust, waiter, mode, FormatRs(amt)],
                oType, "bill", shift, waiter, tbl, cat, mode, oType,
                billNo, $"Bill {billNo} — {cust}", amt,
                new Dictionary<string, string>
                {
                    ["Bill Number"] = billNo,
                    ["Date"] = billDate.ToString("dd-MMM-yyyy"),
                    ["Order Type"] = oType,
                    ["Table / Token"] = tbl,
                    ["Customer"] = cust,
                    ["Waiter"] = waiter,
                    ["Shift"] = shift,
                    ["Primary Category"] = cat,
                    ["Payment Mode"] = mode,
                    ["Net Payable"] = FormatRs(amt)
                }));
        }

        var filtered = ApplyFilters(seeds, filter);
        var sumAmt = filtered.Sum(r => r.NumericMetric);
        var avgAmt = filtered.Count > 0 ? sumAmt / filtered.Count : 0m;

        var kpis = new List<ReportKpiCardDto>
        {
            new("Total Bills", filtered.Count.ToString("N0"), "Server-side paged register", "plum", "bi-receipt"),
            new("Total Collection", FormatRs(sumAmt), "Matching active filters", "teal", "bi-currency-rupee"),
            new("Average Ticket", FormatRs(avgAmt), "Per settled invoice", "saffron", "bi-calculator"),
            new("Digital Share", $"{Pct(filtered.Count(r => r.PaymentMode is "UPI" or "Card"), filtered.Count):0.0}%", "UPI + Card bills", "teal", "bi-qr-code")
        };

        var totals = new List<string> { $"Total ({filtered.Count})", "", "", "", "", "", "", FormatRs(sumAmt) };
        return new(["Bill #", "Date", "Order Type", "Table / Token", "Customer", "Waiter", "Payment", "Total (₹)"], filtered, totals, kpis, null, "Order Type");
    }

    private GeneratedReportPayload BuildTableCollectionReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["T1", "AC Hall", "18", "56", "₹16,840.00", "₹935.56", "Suresh Patil"], "AC Hall", "table", "Evening", "Suresh Patil", "T1", "Main Course", "UPI", "Dine-in", "T1", "Table T1 — AC Hall", 16840m),
            Row(["T2", "AC Hall", "22", "74", "₹24,620.00", "₹1,119.09", "Suresh Patil"], "AC Hall", "table", "Evening", "Suresh Patil", "T2", "Biryani & Rice", "Card", "Dine-in", "T2", "Table T2 — AC Hall", 24620m),
            Row(["T3", "AC Hall", "14", "34", "₹11,290.00", "₹806.43", "Anil Deshmukh"], "AC Hall", "table", "Morning", "Anil Deshmukh", "T3", "Starters", "Cash", "Dine-in", "T3", "Table T3 — AC Hall", 11290m),
            Row(["T4", "AC Hall", "19", "88", "₹31,450.00", "₹1,655.26", "Meena Jadhav"], "AC Hall", "table", "Evening", "Meena Jadhav", "T4", "Main Course", "UPI", "Dine-in", "T4", "Table T4 — AC Hall", 31450m),
            Row(["T5", "Family Hall", "20", "92", "₹34,800.00", "₹1,740.00", "Meena Jadhav"], "Family Hall", "table", "Evening", "Meena Jadhav", "T5", "Biryani & Rice", "UPI", "Dine-in", "T5", "Table T5 — Family Hall", 34800m),
            Row(["T7", "Garden", "16", "68", "₹22,180.00", "₹1,386.25", "Sachin Kale"], "Garden", "table", "Evening", "Sachin Kale", "T7", "Starters", "Cash", "Dine-in", "T7", "Table T7 — Garden", 22180m),
            Row(["T9", "Garden", "12", "42", "₹14,620.00", "₹1,218.33", "Anil Deshmukh"], "Garden", "table", "Morning", "Anil Deshmukh", "T9", "Beverages", "Udhari", "Dine-in", "T9", "Table T9 — Garden", 14620m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var top = filtered.OrderByDescending(r => r.NumericMetric).FirstOrDefault();

        var kpis = new List<ReportKpiCardDto>
        {
            new("Active Tables", filtered.Count.ToString(), "With settled bills", "plum", "bi-grid-3x3-gap"),
            new("Table Collection", FormatRs(total), "Total dine-in revenue", "teal", "bi-currency-rupee"),
            new("Top Table", top is not null ? $"{top.Cells[0]} ({top.Cells[4]})" : "-", top?.Cells[1] ?? "", "saffron", "bi-trophy"),
            new("Avg per Table", FormatRs(filtered.Count > 0 ? total / filtered.Count : 0m), "Collection per table", "plum", "bi-calculator")
        };

        var chart = new ReportChartDto("bar", "Collection by Table (₹)", filtered.Select(r => r.Cells[0]).ToList(), "Collection (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "", FormatRs(total), "", "" };
        return new(["Table", "Zone", "Bills", "Guests", "Total Collection", "Avg Bill", "Primary Waiter"], filtered, totals, kpis, chart, "Zone");
    }

    private GeneratedReportPayload BuildTableSpentTimeReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["T1", "AC Hall", "12:15 PM", "12:55 PM", "01:02 PM", "47 min", "7 min", "₹860.00"], "AC Hall", "table", "Morning", "Suresh Patil", "T1", "All", "Cash", "Dine-in", "T1", "Table T1 — Turnaround Details", 47m),
            Row(["T2", "AC Hall", "12:30 PM", "01:10 PM", "01:18 PM", "48 min", "8 min", "₹1,240.00"], "AC Hall", "table", "Morning", "Suresh Patil", "T2", "All", "UPI", "Dine-in", "T2", "Table T2 — Turnaround Details", 48m),
            Row(["T4", "AC Hall", "07:45 PM", "08:40 PM", "08:52 PM", "67 min", "12 min", "₹2,420.00"], "AC Hall", "table", "Evening", "Meena Jadhav", "T4", "All", "Card", "Dine-in", "T4", "Table T4 — Turnaround Details", 67m),
            Row(["T5", "Family Hall", "08:00 PM", "09:05 PM", "09:15 PM", "75 min", "10 min", "₹3,480.00"], "Family Hall", "table", "Evening", "Meena Jadhav", "T5", "All", "UPI", "Dine-in", "T5", "Table T5 — Turnaround Details", 75m),
            Row(["T7", "Garden", "08:15 PM", "09:35 PM", "09:47 PM", "92 min", "12 min", "₹2,010.00"], "Garden", "table", "Evening", "Sachin Kale", "T7", "All", "Cash", "Dine-in", "T7", "Table T7 — Turnaround Details", 92m),
            Row(["T9", "Garden", "01:45 PM", "02:22 PM", "02:28 PM", "43 min", "6 min", "₹720.00"], "Garden", "table", "Morning", "Anil Deshmukh", "T9", "All", "UPI", "Dine-in", "T9", "Table T9 — Turnaround Details", 43m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var avg = filtered.Count > 0 ? filtered.Average(r => r.NumericMetric) : 0m;
        var longest = filtered.OrderByDescending(r => r.NumericMetric).FirstOrDefault();

        var kpis = new List<ReportKpiCardDto>
        {
            new("Avg Table Time", $"{avg:0.0} min", "Seated to freed", "plum", "bi-hourglass-split"),
            new("Longest Stay", longest is not null ? $"{longest.Cells[0]} ({longest.Cells[5]})" : "-", longest?.Cells[1] ?? "", "chili", "bi-clock-history"),
            new("Avg Bill-to-Leave", "9.2 min", "Payment to table release", "saffron", "bi-stopwatch"),
            new("Sessions Tracked", filtered.Count.ToString(), "Completed table cycles", "teal", "bi-check2-circle")
        };

        var chart = new ReportChartDto("bar", "Time Spent per Table (Minutes)", filtered.Select(r => r.Cells[0]).ToList(), "Minutes", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Average", "", "", "", "", $"{avg:0.0} min", "9.2 min", "" };
        return new(["Table", "Zone", "Seated", "Bill Paid", "Freed", "Time on Table", "Bill-to-Leave", "Bill Amount"], filtered, totals, kpis, chart, "Zone");
    }

    private GeneratedReportPayload BuildMenuCardReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["M01", "Paneer Butter Masala", "Main Course", "Veg", "₹280.00", "5%", "₹98.00", "65.0%", "Available"], "Main Course", "item", "All", "All", "All", "Main Course", "All", "All", "M01", "Paneer Butter Masala — Margin", 280m),
            Row(["M02", "Chicken Biryani", "Biryani & Rice", "Non-Veg", "₹320.00", "5%", "₹128.00", "60.0%", "Available"], "Biryani & Rice", "item", "All", "All", "All", "Biryani & Rice", "All", "All", "M02", "Chicken Biryani — Margin", 320m),
            Row(["M03", "Veg Dum Biryani", "Biryani & Rice", "Veg", "₹220.00", "5%", "₹74.00", "66.4%", "Available"], "Biryani & Rice", "item", "All", "All", "All", "Biryani & Rice", "All", "All", "M03", "Veg Dum Biryani — Margin", 220m),
            Row(["M04", "Butter Naan", "Breads", "Veg", "₹45.00", "5%", "₹11.00", "75.6%", "Available"], "Breads", "item", "All", "All", "All", "Breads", "All", "All", "M04", "Butter Naan — Margin", 45m),
            Row(["M05", "Paneer Tikka", "Starters", "Veg", "₹240.00", "5%", "₹88.00", "63.3%", "Available"], "Starters", "item", "All", "All", "All", "Starters", "All", "All", "M05", "Paneer Tikka — Margin", 240m),
            Row(["M06", "Sweet Lassi", "Beverages", "Veg", "₹80.00", "5%", "₹22.00", "72.5%", "Available"], "Beverages", "item", "All", "All", "All", "Beverages", "All", "All", "M06", "Sweet Lassi — Margin", 80m),
            Row(["M07", "Gulab Jamun (2 Pcs)", "Desserts", "Veg", "₹90.00", "5%", "₹28.00", "68.9%", "Available"], "Desserts", "item", "All", "All", "All", "Desserts", "All", "All", "M07", "Gulab Jamun — Margin", 90m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Menu Dishes", filtered.Count.ToString(), "Active catalog items", "plum", "bi-book"),
            new("Avg Selling Price", FormatRs(filtered.Count > 0 ? filtered.Average(r => r.NumericMetric) : 0m), "Across visible dishes", "teal", "bi-tag"),
            new("Avg Gross Margin", "67.4%", "Selling price vs recipe cost", "teal", "bi-graph-up"),
            new("Veg Share", $"{Pct(filtered.Count(r => r.Cells[3] == "Veg"), filtered.Count):0}%", "Vegetarian dishes", "saffron", "bi-flower1")
        };

        var chart = new ReportChartDto("bar", "Selling Price by Dish (₹)", filtered.Select(r => r.Cells[1]).ToList(), "Price (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { $"Total ({filtered.Count})", "", "", "", "", "", "", "67.4% avg", "" };
        return new(["Code", "Dish Name", "Category", "Diet", "Price", "GST", "Est. Cost", "Margin %", "Status"], filtered, totals, kpis, chart, "Category");
    }

    private GeneratedReportPayload BuildCategorySales(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["Main Course", "14", "1,240", "₹2,98,000.00", "36.3%", "₹240.32"], "Main Course", "item", "All", "All", "All", "Main Course", "All", "Dine-in", "Main Course", "Main Course Category Breakdown", 298000m),
            Row(["Biryani & Rice", "6", "980", "₹2,45,000.00", "29.8%", "₹250.00"], "Biryani & Rice", "item", "All", "All", "All", "Biryani & Rice", "All", "Parcel", "Biryani & Rice", "Biryani & Rice Category Breakdown", 245000m),
            Row(["Starters", "8", "620", "₹1,42,600.00", "17.3%", "₹230.00"], "Starters", "item", "All", "All", "All", "Starters", "All", "Dine-in", "Starters", "Starters Category Breakdown", 142600m),
            Row(["Breads", "5", "2,150", "₹75,250.00", "9.2%", "₹35.00"], "Breads", "item", "All", "All", "All", "Breads", "All", "Dine-in", "Breads", "Breads Category Breakdown", 75250m),
            Row(["Beverages", "6", "840", "₹61,150.00", "7.4%", "₹72.80"], "Beverages", "item", "All", "All", "All", "Beverages", "All", "Delivery", "Beverages", "Beverages Category Breakdown", 61150m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Categories", filtered.Count.ToString(), "Contributing to revenue", "plum", "bi-pie-chart"),
            new("Total Category Sales", FormatRs(total), "Excluding GST", "teal", "bi-currency-rupee"),
            new("Top Category", filtered.OrderByDescending(r => r.NumericMetric).FirstOrDefault()?.Cells[0] ?? "-", "Highest gross revenue", "saffron", "bi-star-fill"),
            new("Units Sold", "5,830", "Total portions served", "plum", "bi-bag")
        };

        var chart = new ReportChartDto("doughnut", "Sales Share by Menu Category (₹)", filtered.Select(r => r.Cells[0]).ToList(), "Sales (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "39", "5,830", FormatRs(total), "100%", "" };
        return new(["Category", "Dishes", "Qty Sold", "Sales Value", "Share %", "Avg Price"], filtered, totals, kpis, chart, null);
    }

    private GeneratedReportPayload BuildItemWiseSalesReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["M02", "Chicken Biryani", "Biryani & Rice", "412", "₹320.00", "₹1,31,840.00", "₹6,592.00"], "Biryani & Rice", "item", "Evening", "Suresh Patil", "T2", "Biryani & Rice", "UPI", "Parcel", "M02", "Chicken Biryani Sales Detail", 131840m),
            Row(["M01", "Paneer Butter Masala", "Main Course", "385", "₹280.00", "₹1,07,800.00", "₹5,390.00"], "Main Course", "item", "Evening", "Meena Jadhav", "T4", "Main Course", "UPI", "Dine-in", "M01", "Paneer Butter Masala Sales Detail", 107800m),
            Row(["M03", "Veg Dum Biryani", "Biryani & Rice", "310", "₹220.00", "₹68,200.00", "₹3,410.00"], "Biryani & Rice", "item", "Morning", "Anil Deshmukh", "T1", "Biryani & Rice", "Cash", "Delivery", "M03", "Veg Dum Biryani Sales Detail", 68200m),
            Row(["M05", "Paneer Tikka", "Starters", "268", "₹240.00", "₹64,320.00", "₹3,216.00"], "Starters", "item", "Evening", "Sachin Kale", "T7", "Starters", "Card", "Dine-in", "M05", "Paneer Tikka Sales Detail", 64320m),
            Row(["M04", "Butter Naan", "Breads", "1,290", "₹45.00", "₹58,050.00", "₹2,902.50"], "Breads", "item", "Evening", "Suresh Patil", "T5", "Breads", "Cash", "Dine-in", "M04", "Butter Naan Sales Detail", 58050m),
            Row(["M06", "Sweet Lassi", "Beverages", "420", "₹80.00", "₹33,600.00", "₹1,680.00"], "Beverages", "item", "Morning", "Anil Deshmukh", "T3", "Beverages", "UPI", "Parcel", "M06", "Sweet Lassi Sales Detail", 33600m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Dishes Sold", filtered.Count.ToString(), "Ranked by net revenue", "plum", "bi-list-ol"),
            new("Gross Item Sales", FormatRs(total), "Before GST", "teal", "bi-currency-rupee"),
            new("Best Seller", filtered.OrderByDescending(r => r.NumericMetric).FirstOrDefault()?.Cells[1] ?? "-", "Top revenue item", "saffron", "bi-award"),
            new("GST Output", FormatRs(total * 0.05m), "5% GST on items", "plum", "bi-percent")
        };

        var chart = new ReportChartDto("bar", "Top Selling Dishes (₹)", filtered.Select(r => r.Cells[1]).ToList(), "Sales (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "", "", FormatRs(total), FormatRs(total * 0.05m) };
        return new(["Code", "Item Name", "Category", "Qty Sold", "Rate", "Gross Sales", "GST (5%)"], filtered, totals, kpis, chart, "Category");
    }

    private GeneratedReportPayload BuildParcelReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["P-001", "18-Nov 12:20", "Parcel", "Ramesh Patil", "3 items", "₹20.00", "₹680.00", "UPI", "11 min"], "Parcel", "bill", "Morning", "Counter", "Counter", "Biryani & Rice", "UPI", "Parcel", "P-001", "Parcel Token P-001", 680m),
            Row(["P-002", "18-Nov 13:05", "Parcel", "Walk-in Guest", "2 items", "₹15.00", "₹415.00", "Cash", "9 min"], "Parcel", "bill", "Morning", "Counter", "Counter", "Main Course", "Cash", "Parcel", "P-002", "Parcel Token P-002", 415m),
            Row(["D-014", "18-Nov 19:30", "Delivery", "Pooja Verma", "4 items", "₹30.00", "₹1,140.00", "UPI", "18 min"], "Delivery", "bill", "Evening", "Counter", "Counter", "Biryani & Rice", "UPI", "Delivery", "D-014", "Delivery Order D-014", 1140m),
            Row(["P-003", "18-Nov 20:15", "Parcel", "Amit Deshmukh", "5 items", "₹25.00", "₹1,520.00", "Card", "14 min"], "Parcel", "bill", "Evening", "Counter", "Counter", "Starters", "Card", "Parcel", "P-003", "Parcel Token P-003", 1520m),
            Row(["D-015", "18-Nov 21:00", "Delivery", "Sunita Shinde", "2 items", "₹20.00", "₹590.00", "Udhari", "16 min"], "Delivery", "bill", "Evening", "Counter", "Counter", "Main Course", "Udhari", "Delivery", "D-015", "Delivery Order D-015", 590m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Parcel / Delivery Orders", filtered.Count.ToString(), "Counter & doorstep", "plum", "bi-bag-check"),
            new("Total Revenue", FormatRs(total), "Including packing charges", "teal", "bi-currency-rupee"),
            new("Packing Charges", "₹110.00", "Collected across orders", "saffron", "bi-box-seam"),
            new("Avg Prep & Pack", "13.6 min", "Token issued to handover", "teal", "bi-stopwatch")
        };

        var chart = new ReportChartDto("bar", "Parcel & Delivery Value (₹)", filtered.Select(r => r.Cells[0]).ToList(), "Amount (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "", "", "₹110.00", FormatRs(total), "", "13.6 min avg" };
        return new(["Token #", "Time", "Order Type", "Customer", "Items", "Packing", "Total", "Payment", "Handover SLA"], filtered, totals, kpis, chart, "Order Type");
    }

    private GeneratedReportPayload BuildWaiterCashierSales(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["EMP-005", "Prakash Shinde", "Cashier", "482", "₹4,12,000.00", "₹854.77", "₹1,85,000.00", "₹2,27,000.00"], "Cashier", "staff", "Evening", "Prakash Shinde", "All", "All", "Cash", "All", "EMP-005", "Prakash Shinde — Cashier Summary", 412000m),
            Row(["EMP-001", "Suresh Patil", "Captain", "294", "₹2,84,500.00", "₹967.68", "₹94,000.00", "₹1,90,500.00"], "Captain", "staff", "Evening", "Suresh Patil", "T1", "All", "UPI", "Dine-in", "EMP-001", "Suresh Patil — Captain Summary", 284500m),
            Row(["EMP-003", "Meena Jadhav", "Captain", "210", "₹1,98,000.00", "₹942.85", "₹62,000.00", "₹1,36,000.00"], "Captain", "staff", "Morning", "Meena Jadhav", "T4", "All", "Card", "Dine-in", "EMP-003", "Meena Jadhav — Captain Summary", 198000m),
            Row(["EMP-002", "Anil Deshmukh", "Waiter", "165", "₹1,48,200.00", "₹898.18", "₹48,000.00", "₹1,00,200.00"], "Waiter", "staff", "Morning", "Anil Deshmukh", "T3", "All", "Cash", "Dine-in", "EMP-002", "Anil Deshmukh — Waiter Summary", 148200m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Active Staff", filtered.Count.ToString(), "Waiters, captains & cashiers", "plum", "bi-people"),
            new("Gross Handled", FormatRs(total), "Total billed value", "teal", "bi-currency-rupee"),
            new("Top Performer", filtered.OrderByDescending(r => r.NumericMetric).FirstOrDefault()?.Cells[1] ?? "-", "Highest sales handled", "saffron", "bi-award"),
            new("Avg Ticket", "₹915.87", "Across dine-in tables", "plum", "bi-calculator")
        };

        var chart = new ReportChartDto("bar", "Sales by Staff Member (₹)", filtered.Select(r => r.Cells[1]).ToList(), "Sales (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "1,151", FormatRs(total), "", "", "" };
        return new(["Code", "Staff Name", "Role", "Bills", "Gross Sales", "Avg Ticket", "Cash", "UPI / Card"], filtered, totals, kpis, chart, "Role");
    }

    private GeneratedReportPayload BuildAttendanceReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["EMP-001", "Suresh Patil", "Morning", "24", "1", "0", "1", "6.0 hrs"], "Morning", "staff", "Morning", "Suresh Patil", "All", "All", "All", "All", "EMP-001", "Suresh Patil — Attendance", 24m),
            Row(["EMP-002", "Anil Deshmukh", "Morning", "22", "2", "1", "1", "3.5 hrs"], "Morning", "staff", "Morning", "Anil Deshmukh", "All", "All", "All", "All", "EMP-002", "Anil Deshmukh — Attendance", 22m),
            Row(["EMP-003", "Meena Jadhav", "Evening", "25", "0", "1", "0", "8.0 hrs"], "Evening", "staff", "Evening", "Meena Jadhav", "All", "All", "All", "All", "EMP-003", "Meena Jadhav — Attendance", 25m),
            Row(["EMP-005", "Prakash Shinde", "Evening", "26", "0", "0", "0", "4.0 hrs"], "Evening", "staff", "Evening", "Prakash Shinde", "All", "All", "All", "All", "EMP-005", "Prakash Shinde — Attendance", 26m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Staff Count", filtered.Count.ToString(), "On attendance register", "plum", "bi-people"),
            new("Attendance Rate", "93.3%", "Present + half-day equivalent", "teal", "bi-calendar2-check"),
            new("Total Absences", "3 days", "Unplanned leaves", "chili", "bi-person-x"),
            new("Overtime Logged", "21.5 hrs", "Payable in payroll", "saffron", "bi-clock")
        };

        var chart = new ReportChartDto("bar", "Present Days by Staff", filtered.Select(r => r.Cells[1]).ToList(), "Present Days", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", filtered.Sum(r => r.NumericMetric).ToString("0"), "3", "2", "2", "21.5 hrs" };
        return new(["Code", "Staff Name", "Shift", "Present", "Absent", "Half-Day", "Leave", "Overtime"], filtered, totals, kpis, chart, "Shift");
    }

    private GeneratedReportPayload BuildEmployeeAdvancesReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["EMP-001", "Suresh Patil", "Service", "₹10,000.00", "₹4,000.00", "₹6,000.00", "₹2,000/mo"], "Service", "staff", "Morning", "Suresh Patil", "All", "All", "Cash", "All", "EMP-001", "Suresh Patil — Advance Ledger", 6000m),
            Row(["EMP-002", "Anil Deshmukh", "Service", "₹5,000.00", "₹2,500.00", "₹2,500.00", "₹1,250/mo"], "Service", "staff", "Morning", "Anil Deshmukh", "All", "All", "UPI", "All", "EMP-002", "Anil Deshmukh — Advance Ledger", 2500m),
            Row(["EMP-004", "Raju Chef", "Kitchen", "₹15,000.00", "₹5,000.00", "₹10,000.00", "₹2,500/mo"], "Kitchen", "staff", "Evening", "Raju Chef", "All", "All", "Cash", "All", "EMP-004", "Raju Chef — Advance Ledger", 10000m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var outSum = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Total Disbursed", "₹30,000.00", "Active advance accounts", "plum", "bi-cash-coin"),
            new("Recovered", "₹11,500.00", "Via salary & cash", "teal", "bi-arrow-down-circle"),
            new("Outstanding", FormatRs(outSum), "Receivable from staff", "chili", "bi-exclamation-circle"),
            new("Staff with Balance", filtered.Count.ToString(), "Scheduled auto-recovery", "saffron", "bi-people")
        };

        var chart = new ReportChartDto("bar", "Outstanding Advance by Staff (₹)", filtered.Select(r => r.Cells[1]).ToList(), "Outstanding (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "₹30,000.00", "₹11,500.00", FormatRs(outSum), "" };
        return new(["Code", "Employee", "Department", "Advance Given", "Recovered", "Outstanding", "Instalment"], filtered, totals, kpis, chart, "Department");
    }

    private GeneratedReportPayload BuildRevenueTrend(ReportFilterModel filter)
    {
        var today = _dateTime.UtcNow.AddHours(5).AddMinutes(30).Date;
        var vals = new[] { 106800m, 114100m, 131800m, 175100m, 198500m, 96000m, 108400m };
        var seeds = new List<ReportRowSeed>();
        for (var i = 0; i < vals.Length; i++)
        {
            var d = today.AddDays(-(6 - i));
            var v = vals[i];
            seeds.Add(Row(
                [d.ToString("dd-MMM-yyyy"), d.DayOfWeek.ToString(), FormatRs(v * 0.60m), FormatRs(v * 0.24m), FormatRs(v * 0.16m), FormatRs(v), (95 + i * 18).ToString()],
                null, "bill", "All", "All", "All", "All", "All", "All", d.ToString("yyyy-MM-dd"), $"Daily Summary — {d:dd-MMM-yyyy}", v));
        }

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Period Revenue", FormatRs(total), $"{filtered.Count} days tracked", "plum", "bi-graph-up-arrow"),
            new("Daily Average", FormatRs(filtered.Count > 0 ? total / filtered.Count : 0m), "Mean daily turnover", "teal", "bi-calculator"),
            new("Peak Day", filtered.OrderByDescending(r => r.NumericMetric).FirstOrDefault()?.Cells[0] ?? "-", "Highest daily sales", "saffron", "bi-trophy"),
            new("Total Orders", "1,043", "Dine-in + Parcel + Delivery", "plum", "bi-receipt")
        };

        var chart = new ReportChartDto("line", "Daily Revenue Trend (₹)", filtered.Select(r => r.Cells[0]).ToList(), "Total Sales (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "", "", FormatRs(total), "1,043" };
        return new(["Date", "Day", "Dine-in", "Parcel", "Delivery", "Total Sales", "Orders"], filtered, totals, kpis, chart, null);
    }

    private GeneratedReportPayload BuildPaymentModeSplit(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["UPI", "612", "₹4,48,500.00", "53.2%", "Settled to Bank"], null, "bill", "All", "All", "All", "All", "UPI", "All", "UPI", "UPI Payment Summary", 448500m),
            Row(["Cash", "345", "₹2,46,000.00", "29.1%", "In Till / Safe"], null, "bill", "All", "All", "All", "All", "Cash", "All", "Cash", "Cash Payment Summary", 246000m),
            Row(["Card", "98", "₹88,400.00", "10.5%", "POS Terminal"], null, "bill", "All", "All", "All", "All", "Card", "All", "Card", "Card Payment Summary", 88400m),
            Row(["Udhari", "38", "₹42,100.00", "5.0%", "Customer Ledger"], null, "bill", "All", "All", "All", "All", "Udhari", "All", "Udhari", "Udhari Credit Summary", 42100m),
            Row(["Split", "24", "₹18,675.00", "2.2%", "Multi-Mode"], null, "bill", "All", "All", "All", "All", "Split", "All", "Split", "Split Payment Summary", 18675m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Total Collected", FormatRs(total), "Across selected modes", "plum", "bi-wallet2"),
            new("Digital (UPI+Card)", FormatRs(filtered.Where(r => r.PaymentMode is "UPI" or "Card").Sum(r => r.NumericMetric)), "Bank settlement", "teal", "bi-qr-code"),
            new("Cash in Drawer", FormatRs(filtered.Where(r => r.PaymentMode == "Cash").Sum(r => r.NumericMetric)), "Physical tender", "saffron", "bi-cash"),
            new("Udhari Posted", FormatRs(filtered.Where(r => r.PaymentMode == "Udhari").Sum(r => r.NumericMetric)), "Pending recovery", "chili", "bi-journal-bookmark")
        };

        var chart = new ReportChartDto("doughnut", "Collection by Payment Mode (₹)", filtered.Select(r => r.Cells[0]).ToList(), "Amount (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "1,117", FormatRs(total), "100%", "" };
        return new(["Payment Mode", "Transactions", "Amount Collected", "Share %", "Settlement Status"], filtered, totals, kpis, chart, null);
    }

    private GeneratedReportPayload BuildCreditRecoveryReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["Ramesh Patil", "9876543210", "₹10,000.00", "₹14,200.00", "₹10,000.00", "₹4,200.00", "Active"], "Active", "customer", "All", "All", "All", "All", "Udhari", "Dine-in", "CUST-1", "Ramesh Patil — Credit Ledger", 4200m),
            Row(["Amit Deshmukh", "9823456789", "₹8,000.00", "₹12,800.00", "₹6,000.00", "₹6,800.00", "Near Limit"], "Near Limit", "customer", "All", "All", "All", "All", "Udhari", "Dine-in", "CUST-3", "Amit Deshmukh — Credit Ledger", 6800m),
            Row(["Sunita Shinde", "9765432109", "₹5,000.00", "₹6,400.00", "₹4,900.00", "₹1,500.00", "Active"], "Active", "customer", "All", "All", "All", "All", "Udhari", "Parcel", "CUST-2", "Sunita Shinde — Credit Ledger", 1500m),
            Row(["Vijay Shrestha", "9922334455", "₹5,000.00", "₹5,200.00", "₹5,200.00", "₹0.00", "Cleared"], "Cleared", "customer", "All", "All", "All", "All", "Udhari", "Dine-in", "CUST-4", "Vijay Shrestha — Credit Ledger", 0m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var outSum = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Credit Posted", "₹38,600.00", "Period Udhari sales", "plum", "bi-journal-bookmark"),
            new("Recovered", "₹26,100.00", "Cash / UPI receipts", "teal", "bi-check-circle"),
            new("Net Outstanding", FormatRs(outSum), "Receivable from customers", "chili", "bi-exclamation-triangle"),
            new("Recovery Rate", "67.6%", "Collected vs posted", "saffron", "bi-graph-up")
        };

        var chart = new ReportChartDto("bar", "Outstanding Credit by Customer (₹)", filtered.Select(r => r.Cells[0]).ToList(), "Outstanding (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "₹28,000.00", "₹38,600.00", "₹26,100.00", FormatRs(outSum), "" };
        return new(["Customer", "Mobile", "Credit Limit", "Credit Posted", "Recovered", "Outstanding", "Status"], filtered, totals, kpis, chart, "Status");
    }

    private GeneratedReportPayload BuildCustomerAgeingReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["Ramesh Patil", "9876543210", "₹2,400.00", "₹1,800.00", "₹0.00", "₹0.00", "₹4,200.00", "0-60 Days"], "0-60 Days", "customer", "All", "All", "All", "All", "Udhari", "All", "CUST-1", "Ramesh Patil — Ageing", 4200m),
            Row(["Amit Deshmukh", "9823456789", "₹1,200.00", "₹2,600.00", "₹2,000.00", "₹1,000.00", "₹6,800.00", "90+ Overdue"], "90+ Overdue", "customer", "All", "All", "All", "All", "Udhari", "All", "CUST-3", "Amit Deshmukh — Ageing", 6800m),
            Row(["Sunita Shinde", "9765432109", "₹1,500.00", "₹0.00", "₹0.00", "₹0.00", "₹1,500.00", "Current (0-30)"], "Current (0-30)", "customer", "All", "All", "All", "All", "Udhari", "All", "CUST-2", "Sunita Shinde — Ageing", 1500m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("0–30 Days", "₹5,100.00", "Current receivables", "teal", "bi-calendar-check"),
            new("31–60 Days", "₹4,400.00", "Follow-up due", "saffron", "bi-calendar"),
            new("61–90 Days", "₹2,000.00", "Reminder sent", "chili", "bi-exclamation-circle"),
            new("90+ Days", "₹1,000.00", "Critical overdue", "chili", "bi-shield-exclamation")
        };

        var chart = new ReportChartDto("bar", "Credit Ageing Buckets (₹)", ["0-30 Days", "31-60 Days", "61-90 Days", "90+ Days"], "Outstanding (₹)", [5100m, 4400m, 2000m, 1000m]);
        var totals = new List<string> { "Total", "", "₹5,100.00", "₹4,400.00", "₹2,000.00", "₹1,000.00", FormatRs(total), "" };
        return new(["Customer", "Mobile", "0–30 Days", "31–60 Days", "61–90 Days", "90+ Days", "Total Due", "Risk Bucket"], filtered, totals, kpis, chart, "Risk Bucket");
    }

    private GeneratedReportPayload BuildBillDeliveryLog(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["TAX-1042", "Ramesh Patil", "9876543210", "WhatsApp", "18-Nov 14:31", "Delivered", "-"], "WhatsApp", "bill", "Morning", "All", "T4", "All", "Cash", "Dine-in", "TAX-1042", "Delivery Log — TAX-1042", 1m),
            Row(["TAX-1041", "Pooja Verma", "9822990011", "WhatsApp", "18-Nov 14:15", "Read", "-"], "WhatsApp", "bill", "Morning", "All", "Counter", "All", "UPI", "Parcel", "TAX-1041", "Delivery Log — TAX-1041", 1m),
            Row(["TAX-1039", "Amit Deshmukh", "9823456789", "Email", "18-Nov 13:32", "Delivered", "-"], "Email", "bill", "Morning", "All", "T2", "All", "Udhari", "Dine-in", "TAX-1039", "Delivery Log — TAX-1039", 1m),
            Row(["TAX-1035", "Sunita Shinde", "9765432109", "WhatsApp", "18-Nov 12:45", "Failed", "Number not on WhatsApp"], "WhatsApp", "bill", "Morning", "All", "T5", "All", "Card", "Dine-in", "TAX-1035", "Delivery Log — TAX-1035", 1m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Bills Dispatched", filtered.Count.ToString(), "WhatsApp + Email", "plum", "bi-send"),
            new("Delivered / Read", filtered.Count(r => r.Cells[5] != "Failed").ToString(), "Confirmed delivery", "teal", "bi-check2-all"),
            new("Failed", filtered.Count(r => r.Cells[5] == "Failed").ToString(), "Requires resend", "chili", "bi-x-circle"),
            new("Success Rate", $"{Pct(filtered.Count(r => r.Cells[5] != "Failed"), filtered.Count):0.0}%", "Digital bill SLA", "saffron", "bi-whatsapp")
        };

        var totals = new List<string> { $"Total ({filtered.Count})", "", "", "", "", "", "" };
        return new(["Bill #", "Customer", "Phone", "Channel", "Sent At", "Status", "Failure Reason"], filtered, totals, kpis, null, "Channel");
    }

    private GeneratedReportPayload BuildDiscountsReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["TAX-1039", "18-Nov 13:30", "Amit Deshmukh", "Prakash Shinde", "₹1,260.00", "₹63.00", "5.0%", "Regular Guest", "Manager"], "Prakash Shinde", "bill", "Morning", "Prakash Shinde", "T2", "Main Course", "Udhari", "Dine-in", "TAX-1039", "Discount Detail — TAX-1039", 63m),
            Row(["TAX-1024", "18-Nov 19:10", "Pooja Verma", "Prakash Shinde", "₹1,950.00", "₹195.00", "10.0%", "Birthday Offer", "Manager"], "Prakash Shinde", "bill", "Evening", "Prakash Shinde", "T4", "Biryani & Rice", "UPI", "Dine-in", "TAX-1024", "Discount Detail — TAX-1024", 195m),
            Row(["TAX-1012", "17-Nov 21:15", "Dr. Kulkarni", "Suresh Patil", "₹2,400.00", "₹240.00", "10.0%", "Privilege Card", "Owner"], "Suresh Patil", "bill", "Evening", "Suresh Patil", "T5", "Starters", "Card", "Dine-in", "TAX-1012", "Discount Detail — TAX-1012", 240m),
            Row(["TAX-1005", "17-Nov 20:30", "Walk-in Guest", "Prakash Shinde", "₹820.00", "₹50.00", "6.1%", "Service Delay", "Manager"], "Prakash Shinde", "bill", "Evening", "Prakash Shinde", "Counter", "Parcel", "Cash", "Parcel", "TAX-1005", "Discount Detail — TAX-1005", 50m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var totalDisc = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Discounted Bills", filtered.Count.ToString(), "With concession applied", "plum", "bi-percent"),
            new("Total Discounts", FormatRs(totalDisc), "Revenue concession", "chili", "bi-tag"),
            new("Avg Discount %", "7.8%", "Of bill subtotal", "saffron", "bi-calculator"),
            new("Approvals", "100%", "All manager/owner approved", "teal", "bi-shield-check")
        };

        var chart = new ReportChartDto("bar", "Discount Amount by Bill (₹)", filtered.Select(r => r.Cells[0]).ToList(), "Discount (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "", "", FormatRs(totalDisc), "7.8% avg", "", "" };
        return new(["Bill #", "Date", "Customer", "Cashier", "Bill Amt", "Discount", "Disc %", "Reason", "Approved By"], filtered, totals, kpis, chart, "Cashier");
    }

    private GeneratedReportPayload BuildHourlySalesReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["11:00 - 12:00", "Morning", "10", "₹6,400.00", "25%", "Quiet"], "Morning", "bill", "Morning", "All", "All", "All", "Cash", "Dine-in", "11:00", "11:00 AM Hour Breakdown", 6400m),
            Row(["12:00 - 13:00", "Morning", "26", "₹21,800.00", "65%", "Moderate"], "Morning", "bill", "Morning", "All", "All", "All", "UPI", "Dine-in", "12:00", "12:00 PM Hour Breakdown", 21800m),
            Row(["13:00 - 14:00", "Morning", "44", "₹42,600.00", "94%", "Lunch Peak"], "Morning", "bill", "Morning", "All", "All", "All", "UPI", "Dine-in", "13:00", "01:00 PM Peak Breakdown", 42600m),
            Row(["14:00 - 15:00", "Morning", "25", "₹19,500.00", "55%", "Moderate"], "Morning", "bill", "Morning", "All", "All", "All", "Cash", "Parcel", "14:00", "02:00 PM Hour Breakdown", 19500m),
            Row(["19:00 - 20:00", "Evening", "32", "₹28,400.00", "70%", "Busy"], "Evening", "bill", "Evening", "All", "All", "All", "UPI", "Dine-in", "19:00", "07:00 PM Hour Breakdown", 28400m),
            Row(["20:00 - 21:00", "Evening", "54", "₹56,200.00", "100%", "Super Peak"], "Evening", "bill", "Evening", "All", "All", "All", "Card", "Dine-in", "20:00", "08:00 PM Super Peak", 56200m),
            Row(["21:00 - 22:00", "Evening", "28", "₹24,100.00", "58%", "Moderate"], "Evening", "bill", "Evening", "All", "All", "All", "UPI", "Delivery", "21:00", "09:00 PM Hour Breakdown", 24100m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var peak = filtered.OrderByDescending(r => r.NumericMetric).FirstOrDefault();
        var kpis = new List<ReportKpiCardDto>
        {
            new("Total Hourly Sales", FormatRs(total), "Across active windows", "plum", "bi-bar-chart"),
            new("Busiest Hour", peak?.Cells[0] ?? "-", peak?.Cells[3] ?? "", "saffron", "bi-lightning-charge"),
            new("Total Orders", "219", "Orders across windows", "teal", "bi-receipt"),
            new("Peak Occupancy", "100%", "Dinner peak rush", "chili", "bi-people-fill")
        };

        var chart = new ReportChartDto("bar", "Sales by Hour Window (₹)", filtered.Select(r => r.Cells[0]).ToList(), "Sales (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "219", FormatRs(total), "", "" };
        return new(["Hour Window", "Shift", "Orders", "Sales (₹)", "Occupancy", "Traffic Band"], filtered, totals, kpis, chart, "Shift");
    }

    private GeneratedReportPayload BuildKitchenPerformanceReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["KOT-101", "T1", "Tandoor", "12:20 PM", "2 min", "9 min", "14 min", "On Time"], "Tandoor", "table", "Morning", "Suresh Patil", "T1", "Breads", "All", "Dine-in", "KOT-101", "KOT-101 Kitchen Timeline", 9m),
            Row(["KOT-102", "T2", "Curry", "12:35 PM", "2 min", "11 min", "16 min", "On Time"], "Curry", "table", "Morning", "Suresh Patil", "T2", "Main Course", "All", "Dine-in", "KOT-102", "KOT-102 Kitchen Timeline", 11m),
            Row(["KOT-103", "P-014", "Biryani", "12:45 PM", "2 min", "9 min", "13 min", "Fast"], "Biryani", "table", "Morning", "Counter", "Counter", "Biryani & Rice", "All", "Parcel", "KOT-103", "KOT-103 Kitchen Timeline", 9m),
            Row(["KOT-104", "T5", "Tandoor", "08:05 PM", "4 min", "16 min", "23 min", "Delayed"], "Tandoor", "table", "Evening", "Meena Jadhav", "T5", "Starters", "All", "Dine-in", "KOT-104", "KOT-104 Kitchen Timeline", 16m),
            Row(["KOT-105", "T7", "Curry", "08:20 PM", "2 min", "12 min", "17 min", "On Time"], "Curry", "table", "Evening", "Sachin Kale", "T7", "Main Course", "All", "Dine-in", "KOT-105", "KOT-105 Kitchen Timeline", 12m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var avgPrep = filtered.Count > 0 ? filtered.Average(r => r.NumericMetric) : 0m;
        var kpis = new List<ReportKpiCardDto>
        {
            new("KOTs Processed", filtered.Count.ToString(), "All kitchen stations", "plum", "bi-fire"),
            new("Avg KOT Prep", $"{avgPrep:0.0} min", "Target SLA <= 15 min", "teal", "bi-stopwatch"),
            new("Fastest KOT", "9 min", "KOT-101 / KOT-103", "saffron", "bi-lightning"),
            new("SLA Delayed", filtered.Count(r => r.Cells[7] == "Delayed").ToString(), "Exceeded 15 min", "chili", "bi-exclamation-triangle")
        };

        var chart = new ReportChartDto("bar", "KOT Prep Time (Minutes)", filtered.Select(r => r.Cells[0]).ToList(), "Prep Minutes", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Average", "", "", "", "2.4 min", $"{avgPrep:0.0} min", "16.6 min", "" };
        return new(["KOT #", "Table/Token", "Station", "Sent At", "Wait", "Prep Time", "Total Time", "SLA Status"], filtered, totals, kpis, chart, "Station");
    }

    private GeneratedReportPayload BuildCancelledBillsReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["VOID-012", "ORD-184 / KOT-92", "T3", "Paneer Tikka", "1", "₹240.00", "Anil Deshmukh", "Guest changed mind", "Manager"], "Guest changed mind", "bill", "Morning", "Anil Deshmukh", "T3", "Starters", "Cash", "Dine-in", "VOID-012", "Void Audit — VOID-012", 240m),
            Row(["VOID-011", "ORD-172 / KOT-84", "T5", "Butter Naan", "2", "₹90.00", "Suresh Patil", "Duplicate punch", "Manager"], "Duplicate punch", "bill", "Evening", "Suresh Patil", "T5", "Breads", "UPI", "Dine-in", "VOID-011", "Void Audit — VOID-011", 90m),
            Row(["VOID-010", "TAX-0812", "P-009", "Chicken Biryani", "1", "₹320.00", "Prakash Shinde", "Pickup cancelled", "Owner"], "Pickup cancelled", "bill", "Evening", "Prakash Shinde", "Counter", "Biryani & Rice", "UPI", "Parcel", "VOID-010", "Void Audit — VOID-010", 320m),
            Row(["VOID-009", "ORD-150 / KOT-70", "T7", "Sweet Lassi", "1", "₹80.00", "Meena Jadhav", "Quality complaint", "Manager"], "Quality complaint", "bill", "Morning", "Meena Jadhav", "T7", "Beverages", "Cash", "Dine-in", "VOID-009", "Void Audit — VOID-009", 80m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Voided Items/Bills", filtered.Count.ToString(), "Logged in audit trail", "chili", "bi-x-circle"),
            new("Voided Value", FormatRs(total), "Total cancelled amount", "chili", "bi-currency-rupee"),
            new("Pre-Kitchen Voids", "2", "Zero food waste", "teal", "bi-check-circle"),
            new("Manager Approved", "100%", "Verified authorizations", "plum", "bi-shield-check")
        };

        var chart = new ReportChartDto("bar", "Cancelled Value by Void # (₹)", filtered.Select(r => r.Cells[0]).ToList(), "Amount (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "", "5", FormatRs(total), "", "", "" };
        return new(["Void #", "Order / Bill", "Table/Token", "Item", "Qty", "Amount", "Staff", "Reason", "Approved By"], filtered, totals, kpis, chart, "Reason");
    }

    private GeneratedReportPayload BuildDayClosingReport(ReportFilterModel filter)
    {
        var today = _dateTime.UtcNow.AddHours(5).AddMinutes(30).Date;
        var seeds = new List<ReportRowSeed>
        {
            Row([today.ToString("dd-MMM-yyyy"), "Morning", "Prakash Shinde", "₹5,000.00", "₹42,600.00", "₹42,600.00", "₹0.00", "Closed"], "Morning", "bill", "Morning", "Prakash Shinde", "All", "All", "Cash", "All", "DC-1", "Morning Shift Closing", 42600m),
            Row([today.AddDays(-1).ToString("dd-MMM-yyyy"), "Evening", "Prakash Shinde", "₹5,000.00", "₹68,400.00", "₹68,250.00", "-₹150.00", "Closed"], "Evening", "bill", "Evening", "Prakash Shinde", "All", "All", "Cash", "All", "DC-2", "Evening Shift Closing", 68250m),
            Row([today.AddDays(-1).ToString("dd-MMM-yyyy"), "Morning", "Suresh Patil", "₹5,000.00", "₹38,900.00", "₹38,900.00", "₹0.00", "Closed"], "Morning", "bill", "Morning", "Suresh Patil", "All", "All", "Cash", "All", "DC-3", "Previous Morning Closing", 38900m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Shifts Closed", filtered.Count.ToString(), "Audited register closures", "plum", "bi-clock-history"),
            new("Cash Counted", FormatRs(total), "Physical drawer handover", "teal", "bi-cash-stack"),
            new("Net Variance", "-₹150.00", "Shortage across period", "chili", "bi-exclamation-triangle"),
            new("Exact Matches", "66.7%", "Zero-variance shifts", "saffron", "bi-check2-circle")
        };

        var chart = new ReportChartDto("bar", "Cash Counted per Shift (₹)", filtered.Select(r => $"{r.Cells[0]} {r.Cells[1]}").ToList(), "Cash (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "", "", FormatRs(total), "-₹150.00", "" };
        return new(["Date", "Shift", "Cashier", "Opening Float", "Expected Cash", "Counted Cash", "Variance", "Status"], filtered, totals, kpis, chart, "Shift");
    }

    private GeneratedReportPayload BuildStockReport(ReportFilterModel filter)
    {
        List<ReportRowSeed> seeds;
        lock (_inventoryStore.SyncRoot)
        {
            seeds = _inventoryStore.StockItems.Select(i => Row(
                [i.Code, i.Name, i.Category, $"{i.CurrentStock:0.##} {i.Unit}", $"{i.ReorderLevel:0.##} {i.Unit}", FormatRs(i.LastPurchaseRate), FormatRs(i.StockValue), i.Status],
                i.Category, "item", "All", "All", "All", i.Category, "All", "All", i.Code, $"{i.Name} — Stock Details", i.StockValue,
                new Dictionary<string, string>
                {
                    ["Code"] = i.Code,
                    ["Item Name"] = i.Name,
                    ["Category"] = i.Category,
                    ["Current Stock"] = $"{i.CurrentStock:0.##} {i.Unit}",
                    ["Reorder Level"] = $"{i.ReorderLevel:0.##} {i.Unit}",
                    ["Last Purchase Rate"] = FormatRs(i.LastPurchaseRate),
                    ["Stock Valuation"] = FormatRs(i.StockValue),
                    ["Status"] = i.Status,
                    ["Expiry Date"] = i.ExpiryDate?.ToString("dd-MMM-yyyy") ?? "N/A"
                })).ToList();
        }

        var filtered = ApplyFilters(seeds, filter);
        var val = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Tracked SKUs", filtered.Count.ToString(), "Live Inventory store", "plum", "bi-boxes"),
            new("Total Stock Value", FormatRs(val), "At last purchase rate", "teal", "bi-currency-rupee"),
            new("Low Stock", filtered.Count(r => r.Cells[7] == "Low").ToString(), "Below reorder level", "saffron", "bi-exclamation-triangle"),
            new("Out of Stock", filtered.Count(r => r.Cells[7] == "Out").ToString(), "Immediate restock needed", "chili", "bi-x-octagon")
        };

        var chart = new ReportChartDto("bar", "Stock Valuation by Item (₹)", filtered.Take(8).Select(r => r.Cells[1]).ToList(), "Value (₹)", filtered.Take(8).Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "", "", "", FormatRs(val), "" };
        return new(["Code", "Item", "Category", "Current Stock", "Reorder Level", "Last Rate", "Stock Value", "Status"], filtered, totals, kpis, chart, "Category");
    }

    private GeneratedReportPayload BuildPurchasesAndMovementsReport(ReportFilterModel filter)
    {
        List<ReportRowSeed> seeds;
        lock (_inventoryStore.SyncRoot)
        {
            seeds = _inventoryStore.StockLedger.Select(e => Row(
                [e.EntryDate.ToString("dd-MMM-yyyy"), e.MovementType, e.ReferenceNo, e.ItemName, e.InQty > 0 ? $"+{e.InQty:0.##} {e.Unit}" : "-", e.OutQty > 0 ? $"-{e.OutQty:0.##} {e.Unit}" : "-", $"{e.BalanceQty:0.##} {e.Unit}", FormatRs(e.ValueImpact)],
                e.MovementType, "item", "All", e.Actor, "All", "All", "All", "All", e.ReferenceNo, $"{e.MovementType} — {e.ItemName}", e.ValueImpact)).ToList();
        }

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Movements", filtered.Count.ToString(), "Purchases, wastage & issues", "plum", "bi-truck"),
            new("Total Value Impact", FormatRs(total), "Inward & outward stock", "teal", "bi-currency-rupee"),
            new("Purchases", filtered.Count(r => r.Cells[1] == "Purchase").ToString(), "Inward invoices", "teal", "bi-cart-plus"),
            new("Wastage Entries", filtered.Count(r => r.Cells[1] == "Wastage").ToString(), "Logged losses", "chili", "bi-trash3")
        };

        var chart = new ReportChartDto("bar", "Value Impact by Movement (₹)", filtered.Take(8).Select(r => r.Cells[2]).ToList(), "Value (₹)", filtered.Take(8).Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "", "", "", "", FormatRs(total) };
        return new(["Date", "Movement Type", "Ref #", "Item", "In Qty", "Out Qty", "Balance", "Value Impact"], filtered, totals, kpis, chart, "Movement Type");
    }

    private GeneratedReportPayload BuildExpensesReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["EXP-041", "16-Nov-2026", "Raw Materials", "Green Grocers APMC", "UPI", "Fresh weekly produce", "₹14,500.00"], "Raw Materials", "bill", "Morning", "All", "All", "All", "UPI", "All", "EXP-041", "Expense EXP-041", 14500m),
            Row(["EXP-042", "15-Nov-2026", "Utilities & Gas", "Bharat Gas Agency", "Card", "4x 19kg LPG cylinders", "₹7,600.00"], "Utilities & Gas", "bill", "Morning", "All", "All", "All", "Card", "All", "EXP-042", "Expense EXP-042", 7600m),
            Row(["EXP-043", "14-Nov-2026", "Maintenance", "Cool Care AC", "Cash", "AC Hall servicing", "₹2,800.00"], "Maintenance", "bill", "Evening", "All", "All", "All", "Cash", "All", "EXP-043", "Expense EXP-043", 2800m),
            Row(["EXP-044", "12-Nov-2026", "Packaging", "Super Poly Pack", "UPI", "Parcel boxes & bags", "₹6,200.00"], "Packaging", "bill", "Evening", "All", "All", "All", "UPI", "Parcel", "EXP-044", "Expense EXP-044", 6200m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var total = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Total Outflow", FormatRs(total), $"{filtered.Count} expense vouchers", "chili", "bi-cash-stack"),
            new("Cash Paid", FormatRs(filtered.Where(r => r.PaymentMode == "Cash").Sum(r => r.NumericMetric)), "From petty cash", "saffron", "bi-cash"),
            new("Bank / UPI Paid", FormatRs(filtered.Where(r => r.PaymentMode != "Cash").Sum(r => r.NumericMetric)), "Digital bank outflow", "plum", "bi-bank"),
            new("Largest Head", filtered.OrderByDescending(r => r.NumericMetric).FirstOrDefault()?.Cells[2] ?? "-", "Top cost center", "teal", "bi-pie-chart")
        };

        var chart = new ReportChartDto("bar", "Expenses by Head (₹)", filtered.Select(r => r.Cells[2]).ToList(), "Amount (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "", "", "", "", FormatRs(total) };
        return new(["Voucher #", "Date", "Expense Head", "Vendor", "Payment", "Description", "Amount"], filtered, totals, kpis, chart, "Expense Head");
    }

    private GeneratedReportPayload BuildAccountsSummaryReport(ReportFilterModel filter)
    {
        var liveVouchers = _accountingService.GetPostedVouchersAsync().GetAwaiter().GetResult();
        var seeds = new List<ReportRowSeed>
        {
            Row(["Profit & Loss", "Food & Beverage Revenue", "4100", "₹0.00", "₹8,03,500.00", "Credit"], "Profit & Loss", "bill", "All", "All", "All", "All", "All", "All", "4100", "Account 4100 — Food Revenue", 803500m),
            Row(["Profit & Loss", "Raw Material COGS", "5100", "₹3,85,000.00", "₹0.00", "Debit"], "Profit & Loss", "bill", "All", "All", "All", "All", "All", "All", "5100", "Account 5100 — COGS", 385000m),
            Row(["Balance Sheet", "Cash in Hand", "1110", "₹1,42,000.00", "₹0.00", "Debit"], "Balance Sheet", "bill", "All", "All", "All", "All", "Cash", "All", "1110", "Account 1110 — Cash in Hand", 142000m),
            Row(["Balance Sheet", "Bank / UPI Account", "1120", "₹5,18,400.00", "₹0.00", "Debit"], "Balance Sheet", "bill", "All", "All", "All", "All", "UPI", "All", "1120", "Account 1120 — Bank Account", 518400m),
            Row(["Balance Sheet", "Customer Udhari Debtors", "1130", "₹19,700.00", "₹0.00", "Debit"], "Balance Sheet", "bill", "All", "All", "All", "All", "Udhari", "All", "1130", "Account 1130 — Debtors", 19700m),
            Row(["Balance Sheet", "Accounts Payable - Suppliers", "2110", "₹0.00", "₹24,500.00", "Credit"], "Balance Sheet", "bill", "All", "All", "All", "All", "Credit", "All", "2110", "Account 2110 — Payables", 24500m)
        };

        foreach (var v in liveVouchers.Take(8))
        {
            seeds.Add(Row(
                ["Day Book (Live)", $"{v.VoucherType} — {v.Narration}", v.VoucherNumber, FormatRs(v.TotalDebit), FormatRs(v.TotalCredit), "Balanced"],
                "Day Book (Live)", "bill", "All", "All", "All", "All", "All", "All", v.VoucherNumber, $"Voucher {v.VoucherNumber}", v.TotalDebit));
        }

        var filtered = ApplyFilters(seeds, filter);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Live Vouchers", liveVouchers.Count.ToString(), "Posted via IAccountingService", "plum", "bi-journal-text"),
            new("Trial Balance", "Balanced", "Total Dr == Total Cr", "teal", "bi-check2-all"),
            new("Cash + Bank", "₹6,60,400.00", "Liquid balance", "teal", "bi-bank"),
            new("Net Profit (Est.)", "₹1,48,200.00", "Current period P&L", "saffron", "bi-graph-up-arrow")
        };

        var chart = new ReportChartDto("bar", "Key Ledger Balances (₹)", filtered.Take(6).Select(r => r.Cells[1]).ToList(), "Balance (₹)", filtered.Take(6).Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Summary", $"{filtered.Count} ledger rows", "", "", "", "Balanced" };
        return new(["Statement", "Account / Particulars", "Code / Ref", "Debit", "Credit", "Nature"], filtered, totals, kpis, chart, "Statement");
    }

    private GeneratedReportPayload BuildGstTaxReport(ReportFilterModel filter)
    {
        var seeds = new List<ReportRowSeed>
        {
            Row(["5% (2.5% + 2.5%)", "Restaurant Food Services", "₹7,45,000.00", "₹18,625.00", "₹18,625.00", "₹37,250.00", "₹7,82,250.00"], "5% Slab", "bill", "All", "All", "All", "Main Course", "All", "Dine-in", "GST-5", "GST 5% Filing Breakdown", 37250m),
            Row(["12% (6% + 6%)", "Bakery & Packaged Sweets", "₹42,500.00", "₹2,550.00", "₹2,550.00", "₹5,100.00", "₹47,600.00"], "12% Slab", "bill", "All", "All", "All", "Desserts", "All", "Parcel", "GST-12", "GST 12% Filing Breakdown", 5100m),
            Row(["18% (9% + 9%)", "Aerated Drinks & Events", "₹16,000.00", "₹1,440.00", "₹1,440.00", "₹2,880.00", "₹18,880.00"], "18% Slab", "bill", "All", "All", "All", "Beverages", "All", "Dine-in", "GST-18", "GST 18% Filing Breakdown", 2880m),
            Row(["0% (Exempt)", "Fresh Farm & Unprocessed Items", "₹12,000.00", "₹0.00", "₹0.00", "₹0.00", "₹12,000.00"], "0% Exempt", "bill", "All", "All", "All", "All", "All", "Parcel", "GST-0", "GST Exempt Supplies", 0m)
        };

        var filtered = ApplyFilters(seeds, filter);
        var totalTax = filtered.Sum(r => r.NumericMetric);
        var kpis = new List<ReportKpiCardDto>
        {
            new("Taxable Turnover", "₹8,15,500.00", "GSTR-1 summary", "plum", "bi-file-earmark-spreadsheet"),
            new("CGST Output", FormatRs(totalTax / 2m), "Central GST payable", "teal", "bi-bank"),
            new("SGST Output", FormatRs(totalTax / 2m), "State GST payable", "teal", "bi-bank2"),
            new("Total GST Output", FormatRs(totalTax), "Ready for filing", "saffron", "bi-percent")
        };

        var chart = new ReportChartDto("bar", "GST Collected by Tax Slab (₹)", filtered.Select(r => r.Cells[0]).ToList(), "GST (₹)", filtered.Select(r => r.NumericMetric).ToList());
        var totals = new List<string> { "Total", "", "₹8,15,500.00", FormatRs(totalTax / 2m), FormatRs(totalTax / 2m), FormatRs(totalTax), "₹8,60,730.00" };
        return new(["Tax Slab", "Description", "Taxable Value", "CGST", "SGST", "Total Tax", "Gross Total"], filtered, totals, kpis, chart, "Tax Slab");
    }

    private static List<ReportRowSeed> ApplyFilters(List<ReportRowSeed> rows, ReportFilterModel f)
    {
        return rows.Where(r =>
            (ReportFilterModel.IsAll(f.Shift) || r.Shift.Equals("All", StringComparison.OrdinalIgnoreCase) || r.Shift.Equals(f.Shift, StringComparison.OrdinalIgnoreCase)) &&
            (ReportFilterModel.IsAll(f.Waiter) || r.Waiter.Equals("All", StringComparison.OrdinalIgnoreCase) || r.Waiter.Contains(f.Waiter, StringComparison.OrdinalIgnoreCase)) &&
            (ReportFilterModel.IsAll(f.Table) || r.Table.Equals("All", StringComparison.OrdinalIgnoreCase) || r.Table.Equals(f.Table, StringComparison.OrdinalIgnoreCase)) &&
            (ReportFilterModel.IsAll(f.Category) || r.Category.Equals("All", StringComparison.OrdinalIgnoreCase) || r.Category.Contains(f.Category, StringComparison.OrdinalIgnoreCase)) &&
            (ReportFilterModel.IsAll(f.PaymentMode) || r.PaymentMode.Equals("All", StringComparison.OrdinalIgnoreCase) || r.PaymentMode.Equals(f.PaymentMode, StringComparison.OrdinalIgnoreCase)) &&
            (ReportFilterModel.IsAll(f.OrderType) || r.OrderType.Equals("All", StringComparison.OrdinalIgnoreCase) || r.OrderType.Equals(f.OrderType, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(f.Search) || r.Cells.Any(c => c.Contains(f.Search, StringComparison.OrdinalIgnoreCase))))
            .ToList();
    }

    private static ReportRowSeed Row(
        IReadOnlyList<string> cells,
        string? groupKey,
        string drillDownType,
        string shift,
        string waiter,
        string table,
        string category,
        string paymentMode,
        string orderType,
        string drillKey,
        string drillTitle,
        decimal metric,
        Dictionary<string, string>? customDetails = null)
    {
        var details = customDetails ?? new Dictionary<string, string>
        {
            ["Reference"] = drillKey,
            ["Category / Group"] = groupKey ?? "-",
            ["Shift"] = shift,
            ["Waiter / Staff"] = waiter,
            ["Table / Token"] = table,
            ["Payment Mode"] = paymentMode,
            ["Order Type"] = orderType,
            ["Summary"] = string.Join(" • ", cells.Take(5))
        };

        return new ReportRowSeed(cells, groupKey, drillKey, drillDownType, drillTitle, details, shift, waiter, table, category, paymentMode, orderType, metric);
    }

    private static string FormatRs(decimal amount) =>
        "₹" + amount.ToString("N2", CultureInfo.InvariantCulture);

    private static decimal Pct(int part, int total) =>
        total <= 0 ? 0m : Math.Round(part * 100m / total, 1);

    public sealed record ReportRowSeed(
        IReadOnlyList<string> Cells,
        string? GroupKey,
        string DrillDownKey,
        string DrillDownType,
        string DrillDownTitle,
        IReadOnlyDictionary<string, string> DrillDownDetails,
        string Shift,
        string Waiter,
        string Table,
        string Category,
        string PaymentMode,
        string OrderType,
        decimal NumericMetric);

    public sealed record GeneratedReportPayload(
        IReadOnlyList<string> Headers,
        IReadOnlyList<ReportRowSeed> Rows,
        IReadOnlyList<string> TotalsRow,
        IReadOnlyList<ReportKpiCardDto> Kpis,
        ReportChartDto? Chart,
        string? GroupByLabel);
}
