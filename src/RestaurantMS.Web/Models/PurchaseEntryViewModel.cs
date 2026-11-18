using Microsoft.AspNetCore.Http;
using RestaurantMS.Application.Common.Models.Inventory;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class PurchaseEntryViewModel
{
    public long SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow.AddHours(5).AddMinutes(30).Date;
    public string PaymentType { get; set; } = "Paid";
    public string PaymentMode { get; set; } = "Cash";
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
    public IFormFile? AttachmentFile { get; set; }
    public string? AttachmentUrl { get; set; }
    public List<PurchaseItemRowModel> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }

    public IReadOnlyList<MasterSupplierDto> Suppliers { get; set; } = [];
    public IReadOnlyList<MasterTaxUnitDto> Units { get; set; } = [];
    public IReadOnlyList<MasterTaxUnitDto> TaxRates { get; set; } = [];
    public IReadOnlyList<InventoryStockItemDto> StockItems { get; set; } = [];
    public IReadOnlyList<PurchaseRecordDto> RecentPurchases { get; set; } = [];
    public IReadOnlyList<SupplierLedgerEntryDto> SupplierLedger { get; set; } = [];
}
