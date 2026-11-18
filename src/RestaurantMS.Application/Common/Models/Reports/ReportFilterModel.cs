namespace RestaurantMS.Application.Common.Models.Reports;

/// <summary>
/// Unified filter, date preset, grouping and server-side pagination model for all reports.
/// </summary>
public class ReportFilterModel
{
    public string ReportCode { get; set; } = "RPT01";
    public string DatePreset { get; set; } = "ThisMonth"; // Today, Yesterday, ThisWeek, ThisMonth, LastMonth, ThisFY, Custom
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string Shift { get; set; } = "All";
    public string Waiter { get; set; } = "All";
    public string Table { get; set; } = "All";
    public string Category { get; set; } = "All";
    public string PaymentMode { get; set; } = "All";
    public string OrderType { get; set; } = "All"; // All, Dine-in, Parcel, Delivery
    public string? Search { get; set; }
    public string? GroupBy { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;

    public (DateTime From, DateTime To) ResolveDateRange(DateTime today)
    {
        var anchor = today.Date;
        var preset = NormalizePreset(DatePreset);

        return preset switch
        {
            "Today" => (anchor, anchor),
            "Yesterday" => (anchor.AddDays(-1), anchor.AddDays(-1)),
            "ThisWeek" => (StartOfWeekMonday(anchor), anchor),
            "ThisMonth" => (new DateTime(anchor.Year, anchor.Month, 1), anchor),
            "LastMonth" => ResolveLastMonth(anchor),
            "ThisFY" => ResolveIndianFinancialYear(anchor),
            "Custom" => ResolveCustomRange(anchor),
            _ => (new DateTime(anchor.Year, anchor.Month, 1), anchor)
        };
    }

    public ReportFilterModel Normalized(DateTime today)
    {
        var (from, to) = ResolveDateRange(today);
        var normalizedPreset = NormalizePreset(DatePreset);
        return new ReportFilterModel
        {
            ReportCode = string.IsNullOrWhiteSpace(ReportCode) ? "RPT01" : ReportCode.Trim().ToUpperInvariant(),
            DatePreset = normalizedPreset,
            FromDate = from,
            ToDate = to,
            Shift = NormalizeAll(Shift),
            Waiter = NormalizeAll(Waiter),
            Table = NormalizeAll(Table),
            Category = NormalizeAll(Category),
            PaymentMode = NormalizeAll(PaymentMode),
            OrderType = NormalizeAll(OrderType),
            Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
            GroupBy = string.IsNullOrWhiteSpace(GroupBy) ? null : GroupBy.Trim(),
            PageNumber = Math.Max(1, PageNumber),
            PageSize = Math.Clamp(PageSize <= 0 ? 25 : PageSize, 5, 100)
        };
    }

    public IReadOnlyList<string> GetActiveFilterSummary(DateTime today)
    {
        var n = Normalized(today);
        var parts = new List<string>
        {
            $"Period: {n.FromDate:dd-MMM-yyyy} to {n.ToDate:dd-MMM-yyyy} ({n.DatePreset})"
        };

        if (!IsAll(n.Shift)) parts.Add($"Shift: {n.Shift}");
        if (!IsAll(n.Waiter)) parts.Add($"Waiter: {n.Waiter}");
        if (!IsAll(n.Table)) parts.Add($"Table: {n.Table}");
        if (!IsAll(n.Category)) parts.Add($"Category: {n.Category}");
        if (!IsAll(n.PaymentMode)) parts.Add($"Payment: {n.PaymentMode}");
        if (!IsAll(n.OrderType)) parts.Add($"Order Type: {n.OrderType}");
        if (!string.IsNullOrWhiteSpace(n.Search)) parts.Add($"Search: \"{n.Search}\"");
        return parts;
    }

    public static bool IsAll(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Equals("All", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeAll(string? value) =>
        IsAll(value) ? "All" : value!.Trim();

    private static string NormalizePreset(string? preset)
    {
        var p = (preset ?? string.Empty).Trim().Replace(" ", string.Empty);
        if (p.Equals("Today", StringComparison.OrdinalIgnoreCase)) return "Today";
        if (p.Equals("Yesterday", StringComparison.OrdinalIgnoreCase)) return "Yesterday";
        if (p.Equals("ThisWeek", StringComparison.OrdinalIgnoreCase)) return "ThisWeek";
        if (p.Equals("ThisMonth", StringComparison.OrdinalIgnoreCase)) return "ThisMonth";
        if (p.Equals("LastMonth", StringComparison.OrdinalIgnoreCase)) return "LastMonth";
        if (p.Equals("ThisFY", StringComparison.OrdinalIgnoreCase) || p.Equals("FY", StringComparison.OrdinalIgnoreCase)) return "ThisFY";
        if (p.Equals("Custom", StringComparison.OrdinalIgnoreCase)) return "Custom";
        return "ThisMonth";
    }

    private static DateTime StartOfWeekMonday(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }

    private static (DateTime From, DateTime To) ResolveLastMonth(DateTime anchor)
    {
        var firstOfCurrent = new DateTime(anchor.Year, anchor.Month, 1);
        var lastOfPrev = firstOfCurrent.AddDays(-1);
        var firstOfPrev = new DateTime(lastOfPrev.Year, lastOfPrev.Month, 1);
        return (firstOfPrev, lastOfPrev);
    }

    private static (DateTime From, DateTime To) ResolveIndianFinancialYear(DateTime anchor)
    {
        var fyStartYear = anchor.Month >= 4 ? anchor.Year : anchor.Year - 1;
        var fyStart = new DateTime(fyStartYear, 4, 1);
        return (fyStart, anchor);
    }

    private (DateTime From, DateTime To) ResolveCustomRange(DateTime anchor)
    {
        var from = (FromDate ?? anchor.AddDays(-7)).Date;
        var to = (ToDate ?? anchor).Date;
        return from <= to ? (from, to) : (to, from);
    }
}
