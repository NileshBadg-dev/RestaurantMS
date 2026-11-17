using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IInventoryUiData
{
    Task<PagedResult<StockItemDto>> GetStockItemsAsync(DataTableRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<SupplierDto>> GetSuppliersAsync(DataTableRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockItemDto>> GetLowStockAlertsAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<PurchaseEntryDto>> GetPurchaseEntriesAsync(DataTableRequest request, CancellationToken cancellationToken = default);
    Task RecordPurchaseAsync(string supplierId, string invoiceNumber, decimal amount, CancellationToken cancellationToken = default);
    Task SaveSupplierAsync(string name, string phone, CancellationToken cancellationToken = default);
}
