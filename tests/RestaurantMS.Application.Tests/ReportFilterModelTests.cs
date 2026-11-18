using RestaurantMS.Application.Billing.Services;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Accounting;
using RestaurantMS.Application.Common.Models.Reports;
using RestaurantMS.Application.Inventory.Services;
using RestaurantMS.Application.Reports.Services;
using RestaurantMS.Domain.Constants;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Tests;

public class ReportFilterModelTests
{
    private static readonly DateTime AnchorDate = new(2026, 11, 18); // Wednesday, 18 Nov 2026

    [Fact]
    public void ResolveDateRange_TodayAndYesterday_ReturnExactSingleDayRanges()
    {
        var today = new ReportFilterModel { DatePreset = "Today" }.ResolveDateRange(AnchorDate);
        var yesterday = new ReportFilterModel { DatePreset = "Yesterday" }.ResolveDateRange(AnchorDate);

        Assert.Equal((new DateTime(2026, 11, 18), new DateTime(2026, 11, 18)), today);
        Assert.Equal((new DateTime(2026, 11, 17), new DateTime(2026, 11, 17)), yesterday);
    }

    [Fact]
    public void ResolveDateRange_ThisWeek_StartsOnMonday()
    {
        var range = new ReportFilterModel { DatePreset = "ThisWeek" }.ResolveDateRange(AnchorDate);

        Assert.Equal(new DateTime(2026, 11, 16), range.From); // Monday
        Assert.Equal(new DateTime(2026, 11, 18), range.To);
    }

    [Fact]
    public void ResolveDateRange_ThisMonthAndLastMonth_ReturnExactBoundaries()
    {
        var thisMonth = new ReportFilterModel { DatePreset = "ThisMonth" }.ResolveDateRange(AnchorDate);
        var lastMonth = new ReportFilterModel { DatePreset = "LastMonth" }.ResolveDateRange(AnchorDate);

        Assert.Equal((new DateTime(2026, 11, 1), new DateTime(2026, 11, 18)), thisMonth);
        Assert.Equal((new DateTime(2026, 10, 1), new DateTime(2026, 10, 31)), lastMonth);
    }

    [Fact]
    public void ResolveDateRange_ThisFY_UsesIndianFinancialYearAprilToMarch()
    {
        var novRange = new ReportFilterModel { DatePreset = "ThisFY" }.ResolveDateRange(new DateTime(2026, 11, 18));
        var febRange = new ReportFilterModel { DatePreset = "ThisFY" }.ResolveDateRange(new DateTime(2027, 2, 15));

        Assert.Equal((new DateTime(2026, 4, 1), new DateTime(2026, 11, 18)), novRange);
        Assert.Equal((new DateTime(2026, 4, 1), new DateTime(2027, 2, 15)), febRange);
    }

    [Fact]
    public void ResolveDateRange_CustomWithInvertedDates_AutoSwapsFromAndTo()
    {
        var filter = new ReportFilterModel
        {
            DatePreset = "Custom",
            FromDate = new DateTime(2026, 11, 20),
            ToDate = new DateTime(2026, 11, 10)
        };

        var range = filter.ResolveDateRange(AnchorDate);

        Assert.Equal(new DateTime(2026, 11, 10), range.From);
        Assert.Equal(new DateTime(2026, 11, 20), range.To);
    }

    [Fact]
    public void Normalized_ClampsPagingAndNormalizesFilterSummary()
    {
        var raw = new ReportFilterModel
        {
            ReportCode = " rpt07 ",
            DatePreset = "ThisWeek",
            Shift = " Evening ",
            OrderType = "Parcel",
            PaymentMode = "All",
            PageNumber = -3,
            PageSize = 500
        };

        var norm = raw.Normalized(AnchorDate);
        var summary = norm.GetActiveFilterSummary(AnchorDate);

        Assert.Equal("RPT07", norm.ReportCode);
        Assert.Equal(1, norm.PageNumber);
        Assert.Equal(100, norm.PageSize);
        Assert.Equal("Evening", norm.Shift);
        Assert.Contains(summary, s => s.Contains("Shift: Evening"));
        Assert.Contains(summary, s => s.Contains("Order Type: Parcel"));
        Assert.DoesNotContain(summary, s => s.Contains("Payment:"));
    }

    [Fact]
    public void ReportsService_CatalogPreferencesPersistencePagingAndPermissions_WorkAsExpected()
    {
        var clock = new FixedDateTime(new DateTime(2026, 11, 18, 6, 0, 0, DateTimeKind.Utc));
        var accounting = new FakeAccountingService();
        var billing = new BillingService(accounting, clock);
        var invStore = new InventoryStateStore();
        var stateStore = new ReportsStateStore();
        var generator = new ReportDataGenerator(billing, accounting, invStore, clock);
        var sut = new ReportsService(stateStore, generator, clock);

        // 10 categories & permission filtering
        Assert.Equal(10, sut.GetCategories().Count);
        var fullCatalog = sut.GetCatalog([], isSuperUser: true);
        Assert.Equal(26, fullCatalog.Count);

        var kitchenOnly = sut.GetCatalog([Permissions.Reports.Kitchen], isSuperUser: false);
        Assert.All(kitchenOnly, r => Assert.Equal("Kitchen", r.Category));

        // Favorites & Recently Used
        var added = sut.ToggleFavorite("user1", "RPT13");
        Assert.True(added);
        Assert.Contains("RPT13", sut.GetUserPreferences("user1").FavoriteCodes);

        sut.RecordRecentlyUsed("user1", "RPT07");
        Assert.Equal("RPT07", sut.GetUserPreferences("user1").RecentCodes[0]);

        // Filter persistence per user per report
        sut.SaveUserFilter("user1", "RPT07", new ReportFilterModel { PaymentMode = "UPI", OrderType = "Parcel" });
        var saved = sut.GetSavedUserFilter("user1", "RPT07");
        Assert.NotNull(saved);
        Assert.Equal("UPI", saved!.PaymentMode);
        Assert.Null(sut.GetSavedUserFilter("user2", "RPT07"));

        // Server-side paging on Bill Register (60 rows)
        var page1 = sut.ExecuteReport("RPT07", new ReportFilterModel { PageNumber = 1, PageSize = 10 });
        var page2 = sut.ExecuteReport("RPT07", new ReportFilterModel { PageNumber = 2, PageSize = 10 });
        Assert.True(page1.IsSuccess && page2.IsSuccess);
        Assert.Equal(10, page1.Value.Rows.Count);
        Assert.True(page1.Value.TotalPages >= 6);
        Assert.NotEqual(page1.Value.Rows[0].Cells[0], page2.Value.Rows[0].Cells[0]);

        // CSV Export includes hotel header & applied filters
        var csv = sut.ExportCsv("RPT01", new ReportFilterModel { DatePreset = "ThisMonth" }, "Grand Restaurant");
        Assert.True(csv.IsSuccess);
        Assert.Contains("Grand Restaurant", csv.Value);
        Assert.Contains("Filters", csv.Value);
    }

    private sealed class FakeAccountingService : IAccountingService
    {
        public Task<bool> IsPeriodLockedAsync(DateTime date, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<DateTime?> GetLockDateAsync(CancellationToken cancellationToken = default) => Task.FromResult<DateTime?>(null);
        public Task SetLockDateAsync(DateTime? lockDate, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Result<PostedVoucherDto>> PostVoucherAsync(PostVoucherRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success(new PostedVoucherDto(1, "VCH-1", request.VoucherDate, request.VoucherType, request.Narration, request.ReferenceNumber, 0m, 0m, request.Lines)));
        public Task<IReadOnlyList<PostedVoucherDto>> GetPostedVouchersAsync(DateTime? fromDate = null, DateTime? toDate = null, string? voucherType = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PostedVoucherDto>>([]);
    }

    private sealed class FixedDateTime(DateTime utcNow) : IDateTime
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
