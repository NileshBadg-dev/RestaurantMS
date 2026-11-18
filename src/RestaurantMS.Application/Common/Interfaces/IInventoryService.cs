using RestaurantMS.Application.Common.Models.Inventory;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Common.Interfaces;

/// <summary>
/// Central inventory management service enforcing feature toggle, stock rules, supplier ledger and accounting vouchers.
/// </summary>
public interface IInventoryService
{
    Task<Result<IReadOnlyList<InventoryStockItemDto>>> GetStockItemsAsync(string? search = null, string? statusFilter = null, string? category = null, CancellationToken cancellationToken = default);

    Task<Result<StockSummaryKpiDto>> GetSummaryKpisAsync(CancellationToken cancellationToken = default);

    Task<Result<InventoryStockItemDto>> CreateStockItemAsync(CreateStockItemRequest request, string actor, CancellationToken cancellationToken = default);

    Task<Result<PurchaseRecordDto>> RecordPurchaseAsync(RecordPurchaseRequest request, string actor, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PurchaseRecordDto>>> GetPurchasesAsync(long? supplierId = null, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<SupplierLedgerEntryDto>>> GetSupplierLedgerAsync(long? supplierId = null, CancellationToken cancellationToken = default);

    Task<Result<StockLedgerEntryDto>> RecordAdjustmentAsync(StockAdjustmentRequest request, string actor, CancellationToken cancellationToken = default);

    Task<Result<StockLedgerEntryDto>> RecordWastageAsync(StockWastageRequest request, string actor, CancellationToken cancellationToken = default);

    Task<Result<StockLedgerEntryDto>> RecordConsumptionAsync(StockConsumptionRequest request, string actor, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<InventoryStockItemDto>>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<StockLedgerEntryDto>>> GetStockLedgerAsync(long? stockItemId = null, string? movementType = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}
