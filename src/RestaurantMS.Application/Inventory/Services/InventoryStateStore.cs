using RestaurantMS.Application.Common.Models.Inventory;

namespace RestaurantMS.Application.Inventory.Services;

/// <summary>
/// Thread-safe singleton state container for inventory items, purchases, supplier ledger and stock ledger.
/// </summary>
public class InventoryStateStore
{
    public object SyncRoot { get; } = new();

    public List<InventoryStockItemDto> StockItems { get; }
    public List<PurchaseRecordDto> Purchases { get; } = [];
    public List<SupplierLedgerEntryDto> SupplierLedger { get; } = [];
    public List<StockLedgerEntryDto> StockLedger { get; } = [];

    private long _nextItemId = 10;
    private long _nextPurchaseId = 1;
    private long _nextSupplierLedgerId = 1;
    private long _nextStockLedgerId = 1;

    public InventoryStateStore()
    {
        var today = DateTime.UtcNow.AddHours(5).AddMinutes(30).Date;
        StockItems =
        [
            BuildItem(1, "INV-001", "Basmati Rice (Daawat)", "Grains", "kg", 120m, 40m, 110m, today.AddDays(90), today),
            BuildItem(2, "INV-002", "Fresh Malai Paneer", "Dairy", "kg", 6m, 15m, 340m, today.AddDays(2), today),
            BuildItem(3, "INV-003", "Amul Pasteurized Butter", "Dairy", "kg", 4m, 10m, 520m, today.AddDays(5), today),
            BuildItem(4, "INV-004", "Refined Sunflower Oil", "Oils", "L", 55m, 25m, 145m, today.AddDays(120), today),
            BuildItem(5, "INV-005", "Chicken Curry Cut", "Poultry & Meat", "kg", 0m, 15m, 220m, today.AddDays(1), today),
            BuildItem(6, "INV-006", "Nashik Red Onions", "Vegetables", "kg", 14m, 25m, 42m, today.AddDays(10), today),
            BuildItem(7, "INV-007", "Hybrid Tomatoes", "Vegetables", "kg", 28m, 15m, 38m, today.AddDays(4), today),
            BuildItem(8, "INV-008", "Garam Masala Whole Spices", "Spices", "kg", 12m, 5m, 680m, today.AddDays(180), today),
            BuildItem(9, "INV-009", "Whole Milk (Full Cream)", "Dairy", "L", 0m, 20m, 66m, today.AddDays(1), today)
        ];

        foreach (var item in StockItems.Where(i => i.CurrentStock > 0m))
        {
            StockLedger.Add(new StockLedgerEntryDto(
                NextStockLedgerId(),
                today.AddDays(-2),
                item.Id,
                item.Name,
                item.Unit,
                "Opening",
                $"OB-{item.Code}",
                item.CurrentStock,
                0m,
                item.CurrentStock,
                item.LastPurchaseRate,
                item.StockValue,
                "Opening stock balance",
                "System"));
        }
    }

    public long NextItemId() => _nextItemId++;
    public long NextPurchaseId() => _nextPurchaseId++;
    public long NextSupplierLedgerId() => _nextSupplierLedgerId++;
    public long NextStockLedgerId() => _nextStockLedgerId++;

    public static InventoryStockItemDto BuildItem(
        long id,
        string code,
        string name,
        string category,
        string unit,
        decimal currentStock,
        decimal reorderLevel,
        decimal lastPurchaseRate,
        DateTime? expiryDate,
        DateTime today)
    {
        var stock = Math.Round(currentStock, 3, MidpointRounding.AwayFromZero);
        var rate = Math.Round(Math.Max(0m, lastPurchaseRate), 2, MidpointRounding.AwayFromZero);
        var value = Math.Round(Math.Max(0m, stock) * rate, 2, MidpointRounding.AwayFromZero);
        var status = stock <= 0m ? "Out" : stock <= reorderLevel ? "Low" : "OK";
        var expiringSoon = expiryDate.HasValue && expiryDate.Value.Date <= today.AddDays(7) && stock > 0m;

        return new InventoryStockItemDto(
            id,
            code,
            name,
            category,
            unit,
            stock,
            Math.Round(Math.Max(0m, reorderLevel), 3, MidpointRounding.AwayFromZero),
            rate,
            value,
            expiryDate,
            expiringSoon,
            status,
            true);
    }
}
