using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleInventoryUiData : IInventoryUiData
{
    private static readonly List<StockItemDto> StockItemsList =
    [
        new(1, "Basmati Rice (Daawat)", "Grains", 150m, "kg", 50m, 110m, false),
        new(2, "Paneer (Fresh Malai)", "Dairy", 8m, "kg", 15m, 320m, true),
        new(3, "Amul Butter", "Dairy", 5m, "kg", 10m, 480m, true),
        new(4, "Refined Sunflower Oil", "Oils", 60m, "ltr", 30m, 140m, false),
        new(5, "Chicken (Fresh Curry Cut)", "Meat", 25m, "kg", 20m, 210m, false),
        new(6, "Onions (Nashik Red)", "Vegetables", 12m, "kg", 25m, 40m, true),
        new(7, "Tomatoes (Hybrid)", "Vegetables", 30m, "kg", 20m, 35m, false),
        new(8, "Maida (Flour)", "Grains", 80m, "kg", 30m, 38m, false)
    ];

    private static readonly List<SupplierDto> SuppliersList =
    [
        new(1, "Modern Dairy Suppliers", "Kishore Patil", "9822001122", "27AAAAA1111A1Z1", 4500m),
        new(2, "Shree Ganesh Grain Mart", "Ganesh Agarwal", "9823112233", "27BBBBB2222B1Z2", 12400m),
        new(3, "Fresh Farm Poultry", "Nitin Kadam", "9890445566", null, 2800m),
        new(4, "Venkateshwara Vegetable Wholesalers", "Raju Shinde", "9766554433", null, 1500m)
    ];

    private static readonly List<PurchaseEntryDto> PurchaseEntriesList =
    [
        new(1, "PUR-2026-089", "Modern Dairy Suppliers", "15/11/2026", 4500m, 3, "Unpaid"),
        new(2, "PUR-2026-088", "Shree Ganesh Grain Mart", "14/11/2026", 18500m, 5, "Paid"),
        new(3, "PUR-2026-087", "Fresh Farm Poultry", "14/11/2026", 5600m, 2, "Partially Paid"),
        new(4, "PUR-2026-086", "Venkateshwara Vegetable Wholesalers", "12/11/2026", 3200m, 6, "Paid")
    ];

    public Task<PagedResult<StockItemDto>> GetStockItemsAsync(DataTableRequest request, CancellationToken cancellationToken = default)
    {
        var all = StockItemsList;
        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim().ToLowerInvariant();
            all = all.Where(s => s.Name.ToLowerInvariant().Contains(search) || s.Category.ToLowerInvariant().Contains(search)).ToList();
        }

        var paged = all.Skip(request.Start).Take(request.PageSize).ToList();
        return Task.FromResult(new PagedResult<StockItemDto>(paged, all.Count, request.PageNumber, request.PageSize));
    }

    public Task<PagedResult<SupplierDto>> GetSuppliersAsync(DataTableRequest request, CancellationToken cancellationToken = default)
    {
        var all = SuppliersList;
        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim().ToLowerInvariant();
            all = all.Where(s => s.Name.ToLowerInvariant().Contains(search) || s.ContactPerson.ToLowerInvariant().Contains(search)).ToList();
        }

        var paged = all.Skip(request.Start).Take(request.PageSize).ToList();
        return Task.FromResult(new PagedResult<SupplierDto>(paged, all.Count, request.PageNumber, request.PageSize));
    }

    public Task<IReadOnlyList<StockItemDto>> GetLowStockAlertsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<StockItemDto> low = StockItemsList.Where(s => s.IsLowStock).ToList();
        return Task.FromResult(low);
    }

    public Task<PagedResult<PurchaseEntryDto>> GetPurchaseEntriesAsync(DataTableRequest request, CancellationToken cancellationToken = default)
    {
        var all = PurchaseEntriesList;
        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim().ToLowerInvariant();
            all = all.Where(p => p.InvoiceNo.ToLowerInvariant().Contains(search) || p.SupplierName.ToLowerInvariant().Contains(search)).ToList();
        }

        var paged = all.Skip(request.Start).Take(request.PageSize).ToList();
        return Task.FromResult(new PagedResult<PurchaseEntryDto>(paged, all.Count, request.PageNumber, request.PageSize));
    }

    public Task RecordPurchaseAsync(string supplierId, string invoiceNumber, decimal amount, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SaveSupplierAsync(string name, string phone, CancellationToken cancellationToken = default)
    {
        SuppliersList.Add(new(SuppliersList.Count + 1, name, name, phone, null, 0m));
        return Task.CompletedTask;
    }
}
