using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Accounting;
using RestaurantMS.Application.Common.Models.Inventory;
using RestaurantMS.Application.Inventory.Services;
using RestaurantMS.Domain.Constants;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Tests;

public class InventoryServiceTests
{
    private static readonly DateTime FixedUtc = new(2026, 11, 18, 6, 0, 0, DateTimeKind.Utc); // 11:30 IST on 2026-11-18
    private static readonly DateTime TodayIst = new(2026, 11, 18);

    private static (InventoryService Service, FakeAccountingService Accounting, FakeFeatureService Features, FakeSettingService Settings) CreateSut(
        bool inventoryEnabled = true,
        bool allowNegativeStock = false)
    {
        var store = new InventoryStateStore();
        var accounting = new FakeAccountingService();
        var features = new FakeFeatureService(inventoryEnabled);
        var settings = new FakeSettingService(allowNegativeStock);
        var clock = new FixedDateTime(FixedUtc);
        return (new InventoryService(store, accounting, features, settings, clock), accounting, features, settings);
    }

    [Fact]
    public async Task GetSummaryKpis_ReturnsItemsValueLowOutAndExpiringCounts()
    {
        var (sut, _, _, _) = CreateSut();

        var result = await sut.GetSummaryKpisAsync();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.TotalItems > 0);
        Assert.True(result.Value.TotalStockValue > 0m);
        Assert.True(result.Value.LowStockCount > 0);
        Assert.True(result.Value.OutOfStockCount > 0);
        Assert.True(result.Value.ExpiringSoonCount > 0);
    }

    [Fact]
    public async Task RecordPurchase_ValidCreditPurchase_UpdatesStockSupplierLedgerAndPostsBalancedVoucher()
    {
        var (sut, accounting, _, _) = CreateSut();
        var request = new RecordPurchaseRequest(
            SupplierId: 1,
            SupplierName: "Metro Cash & Carry",
            InvoiceNumber: "INV-2026-501",
            InvoiceDate: TodayIst,
            PaymentType: "Credit",
            PaymentMode: "Credit",
            PaidAmount: 0m,
            AttachmentUrl: "/uploads/purchases/sample.pdf",
            Notes: "Morning delivery",
            Lines: [new PurchaseLineInputDto(1, "Basmati Rice (Daawat)", 20m, "kg", 115m, 5m, 0m)]);

        var result = await sut.RecordPurchaseAsync(request, "admin");

        Assert.True(result.IsSuccess);
        Assert.Equal(2300m, result.Value.Subtotal);
        Assert.Equal(115m, result.Value.TaxTotal);
        Assert.Equal(2415m, result.Value.GrandTotal);
        Assert.Equal("Credit", result.Value.PaymentStatus);
        Assert.Equal(2415m, result.Value.BalanceDue);
        Assert.Single(accounting.PostedRequests);
        Assert.Equal(accounting.PostedRequests[0].Lines.Sum(l => l.Debit), accounting.PostedRequests[0].Lines.Sum(l => l.Credit));

        var ledger = await sut.GetSupplierLedgerAsync(1);
        Assert.True(ledger.IsSuccess);
        Assert.Single(ledger.Value);
        Assert.Equal(2415m, ledger.Value[0].RunningBalance);
    }

    [Fact]
    public async Task RecordPurchase_ValidPaidPurchase_PostsPurchaseAndPaymentInSupplierLedger()
    {
        var (sut, accounting, _, _) = CreateSut();
        var request = new RecordPurchaseRequest(
            SupplierId: 2,
            SupplierName: "Sahyadri Dairy Farm",
            InvoiceNumber: "INV-2026-502",
            InvoiceDate: TodayIst,
            PaymentType: "Paid",
            PaymentMode: "Bank / UPI",
            PaidAmount: 1050m,
            AttachmentUrl: null,
            Notes: null,
            Lines: [new PurchaseLineInputDto(2, "Fresh Malai Paneer", 3m, "kg", 333.33m, 5m, 0m)]);

        var result = await sut.RecordPurchaseAsync(request, "admin");

        Assert.True(result.IsSuccess);
        Assert.Equal("Paid", result.Value.PaymentStatus);
        Assert.Equal(0m, result.Value.BalanceDue);
        Assert.Single(accounting.PostedRequests);
        Assert.Contains(accounting.PostedRequests[0].Lines, l => l.AccountCode == "1120" && l.Credit == result.Value.GrandTotal);

        var supplierLedger = await sut.GetSupplierLedgerAsync(2);
        Assert.Equal(2, supplierLedger.Value.Count);
        Assert.Equal(0m, supplierLedger.Value[0].RunningBalance);
    }

    [Fact]
    public async Task RecordPurchase_MissingSupplier_ReturnsFailure()
    {
        var (sut, _, _, _) = CreateSut();
        var request = new RecordPurchaseRequest(
            SupplierId: 0,
            SupplierName: "",
            InvoiceNumber: "INV-001",
            InvoiceDate: TodayIst,
            PaymentType: "Paid",
            PaymentMode: "Cash",
            PaidAmount: 100m,
            AttachmentUrl: null,
            Notes: null,
            Lines: [new PurchaseLineInputDto(1, "Basmati Rice (Daawat)", 5m, "kg", 100m, 5m, 0m)]);

        var result = await sut.RecordPurchaseAsync(request, "admin");

        Assert.True(result.IsFailure);
        Assert.Contains("Supplier is required", result.Error.Message);
    }

    [Fact]
    public async Task RecordPurchase_DuplicateInvoicePerSupplierBlocked_DifferentSupplierAllowed()
    {
        var (sut, _, _, _) = CreateSut();
        var first = new RecordPurchaseRequest(
            SupplierId: 1,
            SupplierName: "Metro Cash & Carry",
            InvoiceNumber: "INV-DUP-1",
            InvoiceDate: TodayIst,
            PaymentType: "Credit",
            PaymentMode: "Credit",
            PaidAmount: 0m,
            AttachmentUrl: null,
            Notes: null,
            Lines: [new PurchaseLineInputDto(1, "Basmati Rice (Daawat)", 5m, "kg", 100m, 5m, 0m)]);

        var r1 = await sut.RecordPurchaseAsync(first, "admin");
        var r2 = await sut.RecordPurchaseAsync(first, "admin");
        var r3 = await sut.RecordPurchaseAsync(first with { SupplierId = 2, SupplierName = "Sahyadri Dairy Farm" }, "admin");

        Assert.True(r1.IsSuccess);
        Assert.True(r2.IsFailure);
        Assert.Equal("Inventory.DuplicateInvoice", r2.Error.Code);
        Assert.True(r3.IsSuccess);
    }

    [Fact]
    public async Task RecordPurchase_NonPositiveQtyOrRate_IsBlocked()
    {
        var (sut, _, _, _) = CreateSut();
        var badQty = new RecordPurchaseRequest(
            1, "Metro", "INV-Q0", TodayIst, "Paid", "Cash", 0m, null, null,
            [new PurchaseLineInputDto(1, "Basmati Rice", 0m, "kg", 100m, 5m, 0m)]);
        var badRate = new RecordPurchaseRequest(
            1, "Metro", "INV-R0", TodayIst, "Paid", "Cash", 0m, null, null,
            [new PurchaseLineInputDto(1, "Basmati Rice", 5m, "kg", 0m, 5m, 0m)]);

        Assert.True((await sut.RecordPurchaseAsync(badQty, "admin")).IsFailure);
        Assert.True((await sut.RecordPurchaseAsync(badRate, "admin")).IsFailure);
    }

    [Fact]
    public async Task RecordPurchase_FutureInvoiceDate_IsBlocked()
    {
        var (sut, _, _, _) = CreateSut();
        var futureReq = new RecordPurchaseRequest(
            1, "Metro", "INV-FUT", TodayIst.AddDays(1), "Paid", "Cash", 500m, null, null,
            [new PurchaseLineInputDto(1, "Basmati Rice", 5m, "kg", 100m, 5m, 0m)]);

        var result = await sut.RecordPurchaseAsync(futureReq, "admin");

        Assert.True(result.IsFailure);
        Assert.Contains("future", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RecordWastage_MissingReason_IsBlocked()
    {
        var (sut, _, _, _) = CreateSut();
        var result = await sut.RecordWastageAsync(new StockWastageRequest(1, 2m, TodayIst, "Spoilage", ""), "chef");

        Assert.True(result.IsFailure);
        Assert.Contains("Wastage reason is required", result.Error.Message);
    }

    [Fact]
    public async Task RecordWastage_NegativeStockBlockedUnlessSettingAllows()
    {
        // Item 5 (Chicken Curry Cut) starts at 0 stock
        var (blockedSut, _, _, _) = CreateSut(allowNegativeStock: false);
        var blocked = await blockedSut.RecordWastageAsync(new StockWastageRequest(5, 2m, TodayIst, "Expired", "Cold room failure"), "chef");
        Assert.True(blocked.IsFailure);
        Assert.Equal("Inventory.NegativeStock", blocked.Error.Code);

        var (allowedSut, accounting, _, _) = CreateSut(allowNegativeStock: true);
        var allowed = await allowedSut.RecordWastageAsync(new StockWastageRequest(5, 2m, TodayIst, "Expired", "Cold room failure"), "chef");
        Assert.True(allowed.IsSuccess);
        Assert.Equal(-2m, allowed.Value.BalanceQty);
        Assert.Single(accounting.PostedRequests);
    }

    [Fact]
    public async Task RecordConsumption_DeductsStockAndBlocksNegativeWhenDisallowed()
    {
        var (sut, _, _, _) = CreateSut(allowNegativeStock: false);

        // Item 2 (Fresh Malai Paneer) starts at 6 kg
        var ok = await sut.RecordConsumptionAsync(new StockConsumptionRequest(2, 2.5m, TodayIst, "Main Kitchen", "Lunch prep"), "chef");
        Assert.True(ok.IsSuccess);
        Assert.Equal(3.5m, ok.Value.BalanceQty);

        var tooMuch = await sut.RecordConsumptionAsync(new StockConsumptionRequest(2, 10m, TodayIst, "Main Kitchen", "Dinner"), "chef");
        Assert.True(tooMuch.IsFailure);
        Assert.Equal("Inventory.NegativeStock", tooMuch.Error.Code);
    }

    [Fact]
    public async Task RecordAdjustment_PostsBalancedVoucherAndUpdatesLedger()
    {
        var (sut, accounting, _, _) = CreateSut();
        // Item 1 starts at 120 kg -> physical count 115 kg (5 kg shortage * Rs 110 = Rs 550)
        var result = await sut.RecordAdjustmentAsync(new StockAdjustmentRequest(1, 115m, TodayIst, "Physical audit count"), "manager");

        Assert.True(result.IsSuccess);
        Assert.Equal(115m, result.Value.BalanceQty);
        Assert.Equal(5m, result.Value.OutQty);
        Assert.Single(accounting.PostedRequests);
        Assert.Equal(550m, accounting.PostedRequests[0].Lines.Sum(l => l.Debit));
        Assert.Equal(550m, accounting.PostedRequests[0].Lines.Sum(l => l.Credit));
    }

    [Fact]
    public async Task Operations_WhenInventoryFeatureToggleDisabled_RejectRequest()
    {
        var (sut, _, _, _) = CreateSut(inventoryEnabled: false);

        var itemsRes = await sut.GetStockItemsAsync();
        var kpiRes = await sut.GetSummaryKpisAsync();
        var purRes = await sut.RecordPurchaseAsync(
            new RecordPurchaseRequest(1, "Metro", "INV-OFF", TodayIst, "Paid", "Cash", 100m, null, null,
                [new PurchaseLineInputDto(1, "Rice", 1m, "kg", 100m, 5m, 0m)]),
            "admin");

        Assert.True(itemsRes.IsFailure);
        Assert.Equal("Inventory.Disabled", itemsRes.Error.Code);
        Assert.True(kpiRes.IsFailure);
        Assert.True(purRes.IsFailure);
    }

    private sealed class FakeAccountingService : IAccountingService
    {
        public List<PostVoucherRequest> PostedRequests { get; } = [];
        public Task<bool> IsPeriodLockedAsync(DateTime date, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<DateTime?> GetLockDateAsync(CancellationToken cancellationToken = default) => Task.FromResult<DateTime?>(null);
        public Task SetLockDateAsync(DateTime? lockDate, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Result<PostedVoucherDto>> PostVoucherAsync(PostVoucherRequest request, CancellationToken cancellationToken = default)
        {
            var dr = request.Lines.Sum(l => l.Debit);
            var cr = request.Lines.Sum(l => l.Credit);
            if (Math.Abs(dr - cr) >= 0.01m)
            {
                return Task.FromResult(Result.Failure<PostedVoucherDto>("Voucher.Unbalanced", "Unbalanced"));
            }
            PostedRequests.Add(request);
            return Task.FromResult(Result.Success(new PostedVoucherDto(
                PostedRequests.Count, $"VCH-{PostedRequests.Count:D4}", request.VoucherDate, request.VoucherType, request.Narration, request.ReferenceNumber, dr, cr, request.Lines)));
        }
        public Task<IReadOnlyList<PostedVoucherDto>> GetPostedVouchersAsync(DateTime? fromDate = null, DateTime? toDate = null, string? voucherType = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PostedVoucherDto>>([]);
    }

    private sealed class FakeFeatureService(bool inventoryEnabled) : IFeatureService
    {
        public Task<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(featureKey == FeatureKeys.Inventory ? inventoryEnabled : true);
        public Task<IReadOnlyDictionary<string, bool>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, bool>>(new Dictionary<string, bool> { [FeatureKeys.Inventory] = inventoryEnabled });
        public void InvalidateCache() { }
    }

    private sealed class FakeSettingService(bool allowNegative) : ISettingService
    {
        public Task<string> GetSettingAsync(string key, string defaultValue = "", CancellationToken cancellationToken = default) =>
            Task.FromResult(key == "Inventory.AllowNegativeStock" ? (allowNegative ? "true" : "false") : defaultValue);
        public Task<T> GetSettingAsync<T>(string key, T defaultValue = default!, CancellationToken cancellationToken = default) =>
            Task.FromResult(defaultValue);
        public Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>());
        public void InvalidateCache() { }
    }

    private sealed class FixedDateTime(DateTime utcNow) : IDateTime
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
