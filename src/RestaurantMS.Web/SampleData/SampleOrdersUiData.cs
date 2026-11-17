using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleOrdersUiData : IOrdersUiData
{
    private static readonly List<OrderItemMenuDto> MenuItemsList =
    [
        new(1, "101", "Paneer Butter Masala", "पनीर बटर मसाला", "Main Course", 280m, true, "Veg", "https://images.unsplash.com/photo-1631452180519-c014fe946bc7?w=150&q=80", true, 15),
        new(2, "102", "Chicken Biryani", "चिकन बिर्याणी", "Biryani", 320m, false, "NonVeg", "https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=150&q=80", true, 20),
        new(3, "103", "Veg Biryani", "व्हेज बिर्याणी", "Biryani", 220m, true, "Veg", "https://images.unsplash.com/photo-1589302168068-964664d93dc0?w=150&q=80", true, 15),
        new(4, "104", "Egg Biryani", "अंडा बिर्याणी", "Biryani", 240m, false, "Egg", "https://images.unsplash.com/photo-1546833999-b9f581a1996d?w=150&q=80", true, 15),
        new(5, "105", "Butter Naan", "बटर नान", "Breads", 45m, true, "Veg", "https://images.unsplash.com/photo-1601050690597-df0568f70950?w=150&q=80", true, 5),
        new(6, "106", "Tandoori Roti", "तंदूरी रोटी", "Breads", 25m, true, "Veg", "https://images.unsplash.com/photo-1534422298391-e4f8c172dddb?w=150&q=80", true, 5),
        new(7, "107", "Masala Dosa", "मसाला डोसा", "South Indian", 90m, true, "Veg", "https://images.unsplash.com/photo-1668236543090-82eba5ee5976?w=150&q=80", true, 10),
        new(8, "108", "Idli Sambar", "इडली सांबार", "South Indian", 60m, true, "Veg", "https://images.unsplash.com/photo-1589301760014-d929f3979dbc?w=150&q=80", true, 5),
        new(9, "109", "Sweet Lassi", "गोड लस्सी", "Beverages", 80m, true, "Veg", "https://images.unsplash.com/photo-1517256064527-09c73fc73e38?w=150&q=80", true, 5),
        new(10, "110", "Dal Tadka", "दाल तडका", "Main Course", 180m, true, "Veg", "https://images.unsplash.com/photo-1546833999-b9f581a1996d?w=150&q=80", true, 12),
        new(11, "111", "Jeera Rice", "जीरा राईस", "Rice", 140m, true, "Veg", "https://images.unsplash.com/photo-1516714435131-44d6b64dc6a2?w=150&q=80", true, 8),
        new(12, "112", "Egg Curry", "अंडा करी", "Main Course", 210m, false, "Egg", "https://images.unsplash.com/photo-1603894584373-5ac82b2ae398?w=150&q=80", true, 15),
        new(13, "113", "Chicken Tikka", "चिकन टिक्का", "Starters", 290m, false, "NonVeg", "https://images.unsplash.com/photo-1599488615731-7e5c2823ff28?w=150&q=80", true, 15)
    ];

    private static readonly List<OpenOrderDto> OpenOrdersList = [];
    private static int _orderSeq = 201;
    private static readonly object _lock = new();

    public Task<IReadOnlyList<OrderItemMenuDto>> GetMenuForOrderingAsync(string? category = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var result = MenuItemsList.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            result = result.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            result = result.Where(i => i.Name.ToLowerInvariant().Contains(s) || i.MarathiName.Contains(s) || i.ShortCode.ToLowerInvariant().Contains(s));
        }

        return Task.FromResult<IReadOnlyList<OrderItemMenuDto>>(result.ToList());
    }

    public Task<PagedResult<OpenOrderDto>> GetOpenOrdersAsync(DataTableRequest request, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var all = OpenOrdersList.ToList();
            var paged = all.Skip(request.Start).Take(request.PageSize).ToList();
            return Task.FromResult(new PagedResult<OpenOrderDto>(paged, all.Count, request.PageNumber, request.PageSize));
        }
    }

    public static string PlaceNewOrder(string tableOrType, List<BillingItemRowDto> items, string waiterName, decimal billAmount)
    {
        lock (_lock)
        {
            var orderId = $"ORD-{_orderSeq++}";
            var order = new OpenOrderDto(
                orderId,
                tableOrType,
                waiterName,
                DateTime.UtcNow.ToString("h:mm tt"),
                items.Sum(i => i.Quantity),
                billAmount,
                "Preparing"
            );

            OpenOrdersList.Insert(0, order);

            if (tableOrType.StartsWith("Table", StringComparison.OrdinalIgnoreCase) || tableOrType.StartsWith("T", StringComparison.OrdinalIgnoreCase))
            {
                var code = tableOrType.Replace("Table", "").Trim();
                SampleTablesUiData.SetTableOrder(code, items, billAmount);
            }

            // Also send KOT to Kitchen display
            SampleKitchenUiData.AddTicketFromOrder(orderId, tableOrType, items);

            return orderId;
        }
    }

    public static void CloseOrder(string tableOrType)
    {
        lock (_lock)
        {
            OpenOrdersList.RemoveAll(o => o.TableOrType.Equals(tableOrType, StringComparison.OrdinalIgnoreCase) ||
                                          o.TableOrType.Equals($"Table {tableOrType}", StringComparison.OrdinalIgnoreCase));

            if (tableOrType.StartsWith("Table", StringComparison.OrdinalIgnoreCase) || tableOrType.StartsWith("T", StringComparison.OrdinalIgnoreCase))
            {
                var code = tableOrType.Replace("Table", "").Trim();
                SampleTablesUiData.ReleaseTable(code);
            }
        }
    }
}
