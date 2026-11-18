using RestaurantMS.Application.Billing.Services;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Accounting;
using RestaurantMS.Application.Common.Models.Billing;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Tests;

public class BillingServiceTests
{
    private static (BillingService Service, FakeAccountingService Accounting) CreateSut()
    {
        var accounting = new FakeAccountingService();
        var clock = new FixedDateTime(new DateTime(2026, 11, 18, 8, 30, 0, DateTimeKind.Utc));
        return (new BillingService(accounting, clock), accounting);
    }

    private static List<BillLineInputDto> SampleItems() =>
    [
        new("Paneer Butter Masala", "पनीर बटर मसाला", 2, 280m, 5m),
        new("Butter Naan", "बटर नान", 3, 45m, 5m)
    ];

    [Fact]
    public void CalculateTotals_WithDiscountPackingGstAndRounding_ReturnsExactFigures()
    {
        var (sut, _) = CreateSut();
        // Subtotal = 560 + 135 = 695; Discount = 45 -> 650; Packing = 20 -> Taxable = 670
        // GST 5% of 670 = 33.50 (CGST 16.75, SGST 16.75); RawTotal = 703.50 -> GrandTotal = 704, RoundOff = +0.50
        var result = sut.CalculateTotals(new BillCalculationRequest("Parcel", SampleItems(), 45m, 20m, 5m));

        Assert.Equal(695m, result.Subtotal);
        Assert.Equal(45m, result.DiscountAmount);
        Assert.Equal(20m, result.PackingCharge);
        Assert.Equal(670m, result.TaxableAmount);
        Assert.Equal(16.75m, result.CgstAmount);
        Assert.Equal(16.75m, result.SgstAmount);
        Assert.Equal(33.50m, result.TotalTax);
        Assert.Equal(703.50m, result.RawTotal);
        Assert.Equal(0.50m, result.RoundOff);
        Assert.Equal(704m, result.GrandTotal);
    }

    [Fact]
    public async Task SettleBill_CashOverpaid_ComputesChangeAndPostsBalancedVoucher()
    {
        var (sut, accounting) = CreateSut();
        var policy = new BillingValidationPolicyDto();
        // Subtotal 695 + 5% GST (34.75) = 729.75 -> GrandTotal 730, RoundOff +0.25
        var request = new SettleBillRequest(
            IdempotencyKey: "cash-key-1",
            OrderType: "Dine-in",
            TableCode: "T2",
            ParcelToken: null,
            WaiterName: "Sachin",
            Items: SampleItems(),
            DiscountAmount: 0m,
            PackingCharge: 0m,
            GstPercent: 5m,
            PaymentMode: "Cash",
            Payments: [new BillPaymentLineInputDto("Cash", 730m, CashReceived: 1000m)]);

        var result = await sut.SettleBillAsync(request, policy, "admin");

        Assert.True(result.IsSuccess);
        Assert.Equal(730m, result.Value.Totals.GrandTotal);
        Assert.Equal(1000m, result.Value.CashReceivedTotal);
        Assert.Equal(270m, result.Value.ChangeToReturn);
        Assert.Equal("Cash", result.Value.PrimaryPaymentMode);
        Assert.Single(accounting.PostedRequests);
        var posted = accounting.PostedRequests[0];
        Assert.Equal(posted.Lines.Sum(l => l.Debit), posted.Lines.Sum(l => l.Credit));
    }

    [Fact]
    public async Task SettleBill_CashUnderpaid_ReturnsExactValidationMessage()
    {
        var (sut, _) = CreateSut();
        var request = new SettleBillRequest(
            IdempotencyKey: "cash-under-1",
            OrderType: "Dine-in",
            TableCode: "T1",
            ParcelToken: null,
            WaiterName: "Sachin",
            Items: SampleItems(),
            DiscountAmount: 0m,
            PackingCharge: 0m,
            GstPercent: 5m,
            PaymentMode: "Cash",
            Payments: [new BillPaymentLineInputDto("Cash", 730m, CashReceived: 500m)]);

        var result = await sut.SettleBillAsync(request, new BillingValidationPolicyDto(), "admin");

        Assert.True(result.IsFailure);
        Assert.Equal("Received amount is less than the bill total", result.Error.Message);
    }

    [Fact]
    public async Task SettleBill_UpiReferenceRequiredAndMissing_ReturnsExactValidationMessage()
    {
        var (sut, _) = CreateSut();
        var policy = new BillingValidationPolicyDto(UpiReferenceRequired: true);
        var request = new SettleBillRequest(
            IdempotencyKey: "upi-missing-ref",
            OrderType: "Dine-in",
            TableCode: "T1",
            ParcelToken: null,
            WaiterName: "Sachin",
            Items: SampleItems(),
            DiscountAmount: 0m,
            PackingCharge: 0m,
            GstPercent: 5m,
            PaymentMode: "UPI",
            Payments: [new BillPaymentLineInputDto("UPI", 730m, UpiReference: "", UpiConfirmed: true)]);

        var result = await sut.SettleBillAsync(request, policy, "admin");

        Assert.True(result.IsFailure);
        Assert.Equal("UPI reference number is required", result.Error.Message);
    }

    [Fact]
    public async Task SettleBill_CardMissingLast4_ReturnsExactValidationMessage()
    {
        var (sut, _) = CreateSut();
        var policy = new BillingValidationPolicyDto(CardDetailsRequired: true);
        var request = new SettleBillRequest(
            IdempotencyKey: "card-invalid",
            OrderType: "Dine-in",
            TableCode: "T1",
            ParcelToken: null,
            WaiterName: "Sachin",
            Items: SampleItems(),
            DiscountAmount: 0m,
            PackingCharge: 0m,
            GstPercent: 5m,
            PaymentMode: "Card",
            Payments: [new BillPaymentLineInputDto("Card", 730m, CardLast4: "12")]);

        var result = await sut.SettleBillAsync(request, policy, "admin");

        Assert.True(result.IsFailure);
        Assert.Equal("Enter the last 4 digits of the card", result.Error.Message);
    }

    [Fact]
    public async Task SettleBill_SplitMismatch_ReturnsExactRemainingMessage()
    {
        var (sut, _) = CreateSut();
        var request = new SettleBillRequest(
            IdempotencyKey: "split-mismatch",
            OrderType: "Dine-in",
            TableCode: "T1",
            ParcelToken: null,
            WaiterName: "Sachin",
            Items: SampleItems(),
            DiscountAmount: 0m,
            PackingCharge: 0m,
            GstPercent: 5m,
            PaymentMode: "Split",
            Payments:
            [
                new BillPaymentLineInputDto("Cash", 300m, CashReceived: 300m),
                new BillPaymentLineInputDto("UPI", 200m, UpiConfirmed: true)
            ]);

        var result = await sut.SettleBillAsync(request, new BillingValidationPolicyDto(), "admin");

        Assert.True(result.IsFailure);
        Assert.Equal("Total of payments must equal Rs 730.00. Remaining: Rs 230.00", result.Error.Message);
    }

    [Fact]
    public async Task SettleBill_SplitWithUdhari_SucceedsAndPostsMultiDebitBalancedVoucher()
    {
        var (sut, accounting) = CreateSut();
        var request = new SettleBillRequest(
            IdempotencyKey: "split-udhari-1",
            OrderType: "Dine-in",
            TableCode: "T3",
            ParcelToken: null,
            WaiterName: "Sachin",
            Items: SampleItems(),
            DiscountAmount: 0m,
            PackingCharge: 0m,
            GstPercent: 5m,
            PaymentMode: "Split",
            Payments:
            [
                new BillPaymentLineInputDto("Cash", 230m, CashReceived: 230m),
                new BillPaymentLineInputDto("UPI", 200m, UpiReference: "UTR998877", UpiConfirmed: true),
                new BillPaymentLineInputDto("Udhari", 300m)
            ],
            CustomerId: 1,
            CustomerName: "Ramesh Patil",
            CustomerPhone: "9876543210",
            CustomerOutstanding: 1000m,
            CustomerCreditLimit: 5000m,
            CustomerCreditAllowed: true);

        var result = await sut.SettleBillAsync(request, new BillingValidationPolicyDto(UpiReferenceRequired: true), "admin");

        Assert.True(result.IsSuccess);
        Assert.Equal("Split", result.Value.PrimaryPaymentMode);
        Assert.Equal(300m, result.Value.CreditAmount);
        Assert.Equal(1000m, result.Value.PreviousOutstanding);
        Assert.Equal(1300m, result.Value.NewOutstanding);
        Assert.Single(accounting.PostedRequests);
        var voucher = accounting.PostedRequests[0];
        Assert.Equal(voucher.Lines.Sum(l => l.Debit), voucher.Lines.Sum(l => l.Credit));
    }

    [Fact]
    public async Task SettleBill_UdhariWithoutCustomer_ReturnsExactValidationMessage()
    {
        var (sut, _) = CreateSut();
        var request = new SettleBillRequest(
            IdempotencyKey: "udhari-no-cust",
            OrderType: "Dine-in",
            TableCode: "T1",
            ParcelToken: null,
            WaiterName: "Sachin",
            Items: SampleItems(),
            DiscountAmount: 0m,
            PackingCharge: 0m,
            GstPercent: 5m,
            PaymentMode: "Udhari",
            Payments: [new BillPaymentLineInputDto("Udhari", 730m)]);

        var result = await sut.SettleBillAsync(request, new BillingValidationPolicyDto(), "admin");

        Assert.True(result.IsFailure);
        Assert.Equal("Select a customer for Udhari", result.Error.Message);
    }

    [Fact]
    public async Task SettleBill_UdhariCreditDisabledCustomer_ReturnsExactValidationMessage()
    {
        var (sut, _) = CreateSut();
        var request = new SettleBillRequest(
            IdempotencyKey: "udhari-disabled",
            OrderType: "Dine-in",
            TableCode: "T1",
            ParcelToken: null,
            WaiterName: "Sachin",
            Items: SampleItems(),
            DiscountAmount: 0m,
            PackingCharge: 0m,
            GstPercent: 5m,
            PaymentMode: "Udhari",
            Payments: [new BillPaymentLineInputDto("Udhari", 730m)],
            CustomerId: 5,
            CustomerName: "Ganesh Joshi",
            CustomerPhone: "9811223344",
            CustomerCreditAllowed: false);

        var result = await sut.SettleBillAsync(request, new BillingValidationPolicyDto(), "admin");

        Assert.True(result.IsFailure);
        Assert.Equal("This customer is not allowed credit", result.Error.Message);
    }

    [Fact]
    public async Task SettleBill_UdhariOverLimit_BlockedWithoutOverrideAndAllowedWithManagerReason()
    {
        var (sut, _) = CreateSut();
        var baseReq = new SettleBillRequest(
            IdempotencyKey: "udhari-over-1",
            OrderType: "Dine-in",
            TableCode: "T1",
            ParcelToken: null,
            WaiterName: "Sachin",
            Items: SampleItems(),
            DiscountAmount: 0m,
            PackingCharge: 0m,
            GstPercent: 5m,
            PaymentMode: "Udhari",
            Payments: [new BillPaymentLineInputDto("Udhari", 730m)],
            CustomerId: 2,
            CustomerName: "Sunita Shinde",
            CustomerPhone: "9765432109",
            CustomerOutstanding: 4500m,
            CustomerCreditLimit: 5000m,
            CustomerCreditAllowed: true);

        var blocked = await sut.SettleBillAsync(baseReq, new BillingValidationPolicyDto(), "admin");
        Assert.True(blocked.IsFailure);
        Assert.Equal("Credit limit exceeded. Available credit: Rs 500.00", blocked.Error.Message);

        var overridden = await sut.SettleBillAsync(
            baseReq with { IdempotencyKey = "udhari-over-2", IsManagerOverride = true, ManagerOverrideReason = "Regular VIP guest" },
            new BillingValidationPolicyDto(),
            "admin");
        Assert.True(overridden.IsSuccess);
        Assert.Equal(5230m, overridden.Value.NewOutstanding);
    }

    [Fact]
    public async Task SettleBill_ParcelOrder_GeneratesParcelTokenAndRequiresNoTable()
    {
        var (sut, _) = CreateSut();
        var request = new SettleBillRequest(
            IdempotencyKey: "parcel-1",
            OrderType: "Parcel",
            TableCode: null,
            ParcelToken: null,
            WaiterName: "Counter",
            Items: SampleItems(),
            DiscountAmount: 0m,
            PackingCharge: 20m,
            GstPercent: 5m,
            PaymentMode: "Cash",
            Payments: [new BillPaymentLineInputDto("Cash", 751m, CashReceived: 751m)]);

        var result = await sut.SettleBillAsync(request, new BillingValidationPolicyDto(), "admin");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.TableCode);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.ParcelToken));
        Assert.StartsWith("P-", result.Value.ParcelToken);
    }

    [Fact]
    public async Task SettleBill_DuplicateIdempotencyKey_ReturnsSameBillWithoutSecondVoucher()
    {
        var (sut, accounting) = CreateSut();
        var request = new SettleBillRequest(
            IdempotencyKey: "idemp-key-99",
            OrderType: "Dine-in",
            TableCode: "T4",
            ParcelToken: null,
            WaiterName: "Sachin",
            Items: SampleItems(),
            DiscountAmount: 0m,
            PackingCharge: 0m,
            GstPercent: 5m,
            PaymentMode: "Cash",
            Payments: [new BillPaymentLineInputDto("Cash", 730m, CashReceived: 730m)]);

        var first = await sut.SettleBillAsync(request, new BillingValidationPolicyDto(), "admin");
        var second = await sut.SettleBillAsync(request, new BillingValidationPolicyDto(), "admin");

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(first.Value.BillNumber, second.Value.BillNumber);
        Assert.Single(accounting.PostedRequests);
    }

    private sealed class FakeAccountingService : IAccountingService
    {
        public List<PostVoucherRequest> PostedRequests { get; } = [];

        public Task<bool> IsPeriodLockedAsync(DateTime date, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<DateTime?> GetLockDateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<DateTime?>(null);

        public Task SetLockDateAsync(DateTime? lockDate, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<Result<PostedVoucherDto>> PostVoucherAsync(PostVoucherRequest request, CancellationToken cancellationToken = default)
        {
            var dr = request.Lines.Sum(l => l.Debit);
            var cr = request.Lines.Sum(l => l.Credit);
            if (Math.Abs(dr - cr) >= 0.01m)
            {
                return Task.FromResult(Result.Failure<PostedVoucherDto>("Voucher.Unbalanced", $"Unbalanced: Dr {dr} != Cr {cr}"));
            }

            PostedRequests.Add(request);
            return Task.FromResult(Result.Success(new PostedVoucherDto(
                PostedRequests.Count,
                $"VCH-{PostedRequests.Count:D4}",
                request.VoucherDate,
                request.VoucherType,
                request.Narration,
                request.ReferenceNumber,
                dr,
                cr,
                request.Lines)));
        }

        public Task<IReadOnlyList<PostedVoucherDto>> GetPostedVouchersAsync(DateTime? fromDate = null, DateTime? toDate = null, string? voucherType = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PostedVoucherDto>>([]);
    }

    private sealed class FixedDateTime(DateTime utcNow) : IDateTime
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
