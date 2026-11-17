using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleTablesUiData : ITablesUiData
{
    private static readonly List<TableItemDto> TablesList =
    [
        new("T1", "AC hall", 4, "Free", null, null, null),
        new("T2", "AC hall", 4, "Free", null, null, null),
        new("T3", "Family", 6, "Free", null, null, null),
        new("T4", "AC hall", 4, "Free", null, null, null),
        new("T5", "Garden", 6, "Free", null, null, null),
        new("T6", "Garden", 4, "Free", null, null, null),
        new("T7", "Garden", 2, "Free", null, null, null),
        new("T8", "Garden", 4, "Free", null, null, null),
        new("T9", "Family", 6, "Free", null, null, null),
        new("T10", "Family", 4, "Free", null, null, null),
        new("T11", "AC hall", 4, "Free", null, null, null),
        new("T12", "AC hall", 4, "Free", null, null, null)
    ];

    private static readonly Dictionary<string, List<BillingItemRowDto>> TableOrdersMap = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _lock = new();

    public Task<IReadOnlyList<TableItemDto>> GetTablesAsync(string? section = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var result = string.IsNullOrWhiteSpace(section) || section.Equals("All", StringComparison.OrdinalIgnoreCase)
                ? TablesList.ToList()
                : TablesList.Where(t => t.Section.Equals(section, StringComparison.OrdinalIgnoreCase)).ToList();

            return Task.FromResult<IReadOnlyList<TableItemDto>>(result);
        }
    }

    public Task<IReadOnlyList<TableSectionDto>> GetSectionsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<TableSectionDto> sections =
            [
                new("All", TablesList.Count, TablesList.Count(t => t.Status == "Occupied")),
                new("AC hall", TablesList.Count(t => t.Section == "AC hall"), TablesList.Count(t => t.Section == "AC hall" && t.Status == "Occupied")),
                new("Garden", TablesList.Count(t => t.Section == "Garden"), TablesList.Count(t => t.Section == "Garden" && t.Status == "Occupied")),
                new("Family", TablesList.Count(t => t.Section == "Family"), TablesList.Count(t => t.Section == "Family" && t.Status == "Occupied"))
            ];
            return Task.FromResult(sections);
        }
    }

    public Task<TableDetailsDto?> GetTableDetailsAsync(string tableCode, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var table = TablesList.FirstOrDefault(t => t.TableCode.Equals(tableCode, StringComparison.OrdinalIgnoreCase));
            if (table is null) return Task.FromResult<TableDetailsDto?>(null);

            var items = TableOrdersMap.TryGetValue(tableCode, out var orderItems)
                ? orderItems.ToList()
                : new List<BillingItemRowDto>();

            var details = new TableDetailsDto(
                TableCode: table.TableCode,
                Section: table.Section,
                Capacity: table.Capacity,
                Status: table.Status,
                WaiterName: table.Status == "Occupied" ? "Sachin Patil" : "-",
                SeatedAtTime: table.Status == "Occupied" ? DateTime.UtcNow.ToString("h:mm tt") : "-",
                RunningBill: table.CurrentBillAmount ?? 0m,
                Items: items);

            return Task.FromResult<TableDetailsDto?>(details);
        }
    }

    public static void SetTableOrder(string tableCode, List<BillingItemRowDto> items, decimal billAmount)
    {
        lock (_lock)
        {
            TableOrdersMap[tableCode] = new List<BillingItemRowDto>(items);

            var idx = TablesList.FindIndex(t => t.TableCode.Equals(tableCode, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                var cur = TablesList[idx];
                TablesList[idx] = new TableItemDto(cur.TableCode, cur.Section, cur.Capacity, "Occupied", 1, billAmount, null);
            }
        }
    }

    public static void ReleaseTable(string tableCode)
    {
        lock (_lock)
        {
            TableOrdersMap.Remove(tableCode);

            var idx = TablesList.FindIndex(t => t.TableCode.Equals(tableCode, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                var cur = TablesList[idx];
                TablesList[idx] = new TableItemDto(cur.TableCode, cur.Section, cur.Capacity, "Free", null, null, null);
            }
        }
    }

    public static List<BillingItemRowDto> GetItemsForTable(string tableCode)
    {
        lock (_lock)
        {
            return TableOrdersMap.TryGetValue(tableCode, out var items)
                ? new List<BillingItemRowDto>(items)
                : new List<BillingItemRowDto>();
        }
    }
}
