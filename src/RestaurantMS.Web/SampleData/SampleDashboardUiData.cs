using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleDashboardUiData : IDashboardUiData
{
    private readonly ITablesUiData _tablesUiData;

    public SampleDashboardUiData(ITablesUiData tablesUiData)
    {
        _tablesUiData = tablesUiData;
    }

    public async Task<DashboardKpisDto> GetKpisAsync(CancellationToken cancellationToken = default)
    {
        var tables = await _tablesUiData.GetTablesAsync(null, cancellationToken);
        var activeCount = tables.Count(t => t.Status == "Occupied");
        var freeCount = tables.Count(t => t.Status == "Free");
        var reservedCount = tables.Count(t => t.Status == "Reserved");

        var settled = SampleBillingUiData.GetAllSettledBills();
        var totalSales = settled.Sum(b => b.Amount);
        var dineInSales = settled.Where(b => b.OrderType.Equals("Dine-in", StringComparison.OrdinalIgnoreCase)).Sum(b => b.Amount);
        var parcelSales = settled.Where(b => !b.OrderType.Equals("Dine-in", StringComparison.OrdinalIgnoreCase)).Sum(b => b.Amount);
        var dineInCount = settled.Count(b => b.OrderType.Equals("Dine-in", StringComparison.OrdinalIgnoreCase));
        var parcelCount = settled.Count(b => !b.OrderType.Equals("Dine-in", StringComparison.OrdinalIgnoreCase));

        var result = new DashboardKpisDto(
            TodaySales: totalSales,
            YesterdayComparisonPercent: 0.0m,
            OrdersDone: settled.Count,
            DineInOrdersDone: dineInCount,
            DineInSales: dineInSales,
            ParcelsDone: parcelCount,
            ParcelSales: parcelSales,
            PendingParcelPickups: 0,
            ActiveTables: activeCount,
            TotalTables: tables.Count,
            FreeTables: freeCount,
            ReservedTables: reservedCount,
            AvgTableMinutes: activeCount > 0 ? 30 : 0,
            LongestTableInfo: activeCount > 0 ? "T1 · 30m" : "-");

        return result;
    }

    public Task<IReadOnlyList<HourlySalesPointDto>> GetSalesByHourAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<HourlySalesPointDto> points =
        [
            new("11", 0m, 0m),
            new("12", 0m, 0m),
            new("1", 0m, 0m),
            new("2", 0m, 0m),
            new("3", 0m, 0m),
            new("4", 0m, 0m),
            new("5", 0m, 0m),
            new("6", 0m, 0m),
            new("7", 0m, 0m),
            new("8", 0m, 0m),
            new("9", 0m, 0m),
            new("10", 0m, 0m)
        ];
        return Task.FromResult(points);
    }

    public Task<IReadOnlyList<TopSellingItemDto>> GetTopSellingItemsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TopSellingItemDto> items =
        [
            new("Paneer Butter Masala", 0, "#C62839"),
            new("Chicken Biryani", 0, "#FF9F1C"),
            new("Masala Dosa", 0, "#0B7A70"),
            new("Butter Naan", 0, "#3D1F6D"),
            new("Sweet Lassi", 0, "#7B7495")
        ];
        return Task.FromResult(items);
    }

    public Task<IReadOnlyList<SalesTrendPointDto>> GetLast7DaysSalesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SalesTrendPointDto> points =
        [
            new("Mon", 0m),
            new("Tue", 0m),
            new("Wed", 0m),
            new("Thu", 0m),
            new("Fri", 0m),
            new("Sat", 0m),
            new("Sun", 0m)
        ];
        return Task.FromResult(points);
    }

    public Task<TableTimeStatsDto> GetTableTimeStatsAsync(CancellationToken cancellationToken = default)
    {
        var sections = new Dictionary<string, int>
        {
            ["AC hall"] = 0,
            ["Garden"] = 0,
            ["Family"] = 0
        };
        var stats = new TableTimeStatsDto(0, 0, 0, sections);
        return Task.FromResult(stats);
    }

    public Task<DineInVsParcelSplitDto> GetDineInVsParcelSplitAsync(CancellationToken cancellationToken = default)
    {
        var settled = SampleBillingUiData.GetAllSettledBills();
        var dineInCount = settled.Count(b => b.OrderType.Equals("Dine-in", StringComparison.OrdinalIgnoreCase));
        var parcelCount = settled.Count(b => !b.OrderType.Equals("Dine-in", StringComparison.OrdinalIgnoreCase));
        var total = settled.Count;

        var dineInPct = total > 0 ? (dineInCount * 100 / total) : 50;
        var parcelPct = total > 0 ? (parcelCount * 100 / total) : 50;
        var dineInAvg = dineInCount > 0 ? settled.Where(b => b.OrderType == "Dine-in").Average(b => b.Amount) : 0m;
        var parcelAvg = parcelCount > 0 ? settled.Where(b => b.OrderType != "Dine-in").Average(b => b.Amount) : 0m;

        var split = new DineInVsParcelSplitDto(dineInPct, parcelPct, dineInAvg, parcelAvg);
        return Task.FromResult(split);
    }

    public async Task<PagedResult<ActiveTableItemDto>> GetActiveTablesAsync(DataTableRequest request, CancellationToken cancellationToken = default)
    {
        var tables = await _tablesUiData.GetTablesAsync(null, cancellationToken);
        var occupied = tables.Where(t => t.Status == "Occupied").Select(t => new ActiveTableItemDto(
            t.TableCode,
            t.Section,
            "Sachin",
            t.Capacity,
            DateTime.UtcNow.ToString("h:mm tt"),
            $"{t.ElapsedMinutes ?? 1}m",
            t.CurrentBillAmount ?? 0m,
            "Occupied"
        )).ToList();

        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim().ToLowerInvariant();
            occupied = occupied.Where(t => t.TableCode.ToLowerInvariant().Contains(search) ||
                                           t.SectionName.ToLowerInvariant().Contains(search) ||
                                           t.WaiterName.ToLowerInvariant().Contains(search)).ToList();
        }

        var paged = occupied.Skip(request.Start).Take(request.PageSize).ToList();
        return new PagedResult<ActiveTableItemDto>(paged, occupied.Count, request.PageNumber, request.PageSize);
    }

    public Task<PagedResult<CompletedOrderItemDto>> GetOrdersDoneTodayAsync(DataTableRequest request, CancellationToken cancellationToken = default)
    {
        var settled = SampleBillingUiData.GetAllSettledBills();
        var items = settled.Select(b => new CompletedOrderItemDto(
            b.BillNo,
            b.DateTimeFormatted,
            b.OrderType,
            b.TableOrCustomer,
            "Items",
            b.Amount,
            b.PaymentMode
        )).ToList();

        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim().ToLowerInvariant();
            items = items.Where(o => o.BillNo.ToLowerInvariant().Contains(search) ||
                                     o.TableOrCustomer.ToLowerInvariant().Contains(search) ||
                                     o.OrderType.ToLowerInvariant().Contains(search)).ToList();
        }

        var paged = items.Skip(request.Start).Take(request.PageSize).ToList();
        return Task.FromResult(new PagedResult<CompletedOrderItemDto>(paged, items.Count, request.PageNumber, request.PageSize));
    }
}
