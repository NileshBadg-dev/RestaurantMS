using System.Globalization;
using System.Text.RegularExpressions;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Accounting;
using RestaurantMS.Application.Common.Models.Billing;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Billing.Services;

public class BillingService : IBillingService
{
    private static readonly Regex FourDigitsRegex = new(@"^\d{4}$", RegexOptions.Compiled);

    private readonly IAccountingService _accountingService;
    private readonly IDateTime _dateTime;
    private readonly object _lock = new();
    private readonly Dictionary<string, SettledBillReceiptDto> _byIdempotencyKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, SettledBillReceiptDto> _byBillNumber = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<SettledBillReceiptDto> _receipts = [];
    private int _billSequence = 1001;
    private int _parcelTokenSequence = 1;

    public BillingService(IAccountingService accountingService, IDateTime dateTime)
    {
        _accountingService = accountingService;
        _dateTime = dateTime;
    }

    public BillCalculationResult CalculateTotals(BillCalculationRequest request)
    {
        var items = request.Items ?? [];
        var subtotal = 0m;
        foreach (var item in items)
        {
            if (item.Quantity <= 0 || item.Rate < 0m)
            {
                continue;
            }

            subtotal += Math.Round(item.Rate * item.Quantity, 2, MidpointRounding.AwayFromZero);
        }

        var discount = Math.Clamp(Math.Round(request.DiscountAmount, 2, MidpointRounding.AwayFromZero), 0m, subtotal);
        var packing = Math.Max(0m, Math.Round(request.PackingCharge, 2, MidpointRounding.AwayFromZero));
        var taxable = Math.Max(0m, Math.Round(subtotal - discount + packing, 2, MidpointRounding.AwayFromZero));
        var gstPercent = Math.Clamp(request.GstPercent, 0m, 28m);

        var totalTax = Math.Round(taxable * (gstPercent / 100m), 2, MidpointRounding.AwayFromZero);
        var cgst = Math.Round(totalTax / 2m, 2, MidpointRounding.AwayFromZero);
        var sgst = Math.Round(totalTax - cgst, 2, MidpointRounding.AwayFromZero);

        var rawTotal = Math.Round(taxable + totalTax, 2, MidpointRounding.AwayFromZero);
        var grandTotal = Math.Max(0m, Math.Round(rawTotal, 0, MidpointRounding.AwayFromZero));
        var roundOff = Math.Round(grandTotal - rawTotal, 2, MidpointRounding.AwayFromZero);

        return new BillCalculationResult(
            subtotal,
            discount,
            packing,
            taxable,
            gstPercent,
            cgst,
            sgst,
            totalTax,
            rawTotal,
            roundOff,
            grandTotal);
    }

    public Result ValidateSettlement(SettleBillRequest request, BillingValidationPolicyDto policy)
    {
        var validItems = (request.Items ?? []).Where(i => i.Quantity > 0 && i.Rate >= 0m).ToList();
        if (validItems.Count == 0)
        {
            return Result.Failure("Billing.EmptyItems", "Add at least one item to settle the bill");
        }

        var totals = CalculateTotals(new BillCalculationRequest(
            request.OrderType,
            validItems,
            request.DiscountAmount,
            request.PackingCharge,
            request.GstPercent));

        if (totals.GrandTotal <= 0m)
        {
            return Result.Failure("Billing.InvalidTotal", "Bill total must be greater than zero");
        }

        var payments = NormalizePayments(request, totals.GrandTotal);
        if (payments.Count == 0)
        {
            return Result.Failure("Billing.NoPayment", "Select a payment mode to settle the bill");
        }

        foreach (var line in payments)
        {
            var mode = NormalizeMode(line.Mode);
            if (line.Amount <= 0m)
            {
                return Result.Failure("Billing.InvalidPaymentAmount", "Payment amount must be greater than zero");
            }

            if (mode == "Cash")
            {
                var received = line.CashReceived ?? line.Amount;
                if (received < line.Amount)
                {
                    return Result.Failure("Billing.CashUnderpaid", "Received amount is less than the bill total");
                }
            }
            else if (mode == "UPI")
            {
                if (policy.UpiReferenceRequired && string.IsNullOrWhiteSpace(line.UpiReference))
                {
                    return Result.Failure("Billing.UpiRefRequired", "UPI reference number is required");
                }
            }
            else if (mode == "Card")
            {
                var last4 = line.CardLast4?.Trim() ?? string.Empty;
                if (policy.CardDetailsRequired || !string.IsNullOrEmpty(last4))
                {
                    if (!FourDigitsRegex.IsMatch(last4))
                    {
                        return Result.Failure("Billing.CardLast4Required", "Enter the last 4 digits of the card");
                    }
                }
            }
            else if (mode == "Udhari")
            {
                var hasCustomer = (request.CustomerId.HasValue && request.CustomerId.Value > 0)
                                  || (!string.IsNullOrWhiteSpace(request.CustomerName)
                                      && !request.CustomerName.Equals("Walk-in Customer", StringComparison.OrdinalIgnoreCase)
                                      && !string.IsNullOrWhiteSpace(request.CustomerPhone));
                if (!hasCustomer)
                {
                    return Result.Failure("Billing.UdhariCustomerRequired", "Select a customer for Udhari");
                }

                if (!request.CustomerCreditAllowed)
                {
                    return Result.Failure("Billing.CreditDisabled", "This customer is not allowed credit");
                }
            }
        }

        var totalPaid = Math.Round(payments.Sum(p => p.Amount), 2, MidpointRounding.AwayFromZero);
        var remaining = Math.Round(totals.GrandTotal - totalPaid, 2, MidpointRounding.AwayFromZero);

        if (Math.Abs(remaining) >= 0.01m)
        {
            var msg = string.Format(
                CultureInfo.InvariantCulture,
                "Total of payments must equal Rs {0:0.00}. Remaining: Rs {1:0.00}",
                totals.GrandTotal,
                remaining);
            return Result.Failure("Billing.PaymentMismatch", msg);
        }

        var totalUdhari = payments
            .Where(p => NormalizeMode(p.Mode) == "Udhari")
            .Sum(p => p.Amount);

        if (totalUdhari > 0m)
        {
            var availableCredit = Math.Max(0m, Math.Round(request.CustomerCreditLimit - request.CustomerOutstanding, 2, MidpointRounding.AwayFromZero));
            if (totalUdhari > availableCredit)
            {
                if (!request.IsManagerOverride || string.IsNullOrWhiteSpace(request.ManagerOverrideReason))
                {
                    var msg = string.Format(
                        CultureInfo.InvariantCulture,
                        "Credit limit exceeded. Available credit: Rs {0:0.00}",
                        availableCredit);
                    return Result.Failure("Billing.CreditLimitExceeded", msg);
                }
            }
        }

        return Result.Success();
    }

    public async Task<Result<SettledBillReceiptDto>> SettleBillAsync(
        SettleBillRequest request,
        BillingValidationPolicyDto policy,
        string cashierName,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
            ? Guid.NewGuid().ToString("N")
            : request.IdempotencyKey.Trim();

        lock (_lock)
        {
            if (_byIdempotencyKey.TryGetValue(idempotencyKey, out var existing))
            {
                return Result.Success(existing);
            }
        }

        var validation = ValidateSettlement(request, policy);
        if (validation.IsFailure)
        {
            return Result.Failure<SettledBillReceiptDto>(validation.Error);
        }

        var validItems = request.Items.Where(i => i.Quantity > 0 && i.Rate >= 0m).ToList();
        var totals = CalculateTotals(new BillCalculationRequest(
            request.OrderType,
            validItems,
            request.DiscountAmount,
            request.PackingCharge,
            request.GstPercent));

        var normalizedPayments = NormalizePayments(request, totals.GrandTotal);
        var settledAtUtc = _dateTime.UtcNow;

        string billNumber;
        string? parcelToken = request.ParcelToken;
        lock (_lock)
        {
            if (_byIdempotencyKey.TryGetValue(idempotencyKey, out var existing))
            {
                return Result.Success(existing);
            }

            billNumber = $"B-{_billSequence++}";
            if (IsParcelOrder(request.OrderType) && string.IsNullOrWhiteSpace(parcelToken))
            {
                parcelToken = $"P-{_parcelTokenSequence++:D3}";
            }
        }

        var voucherResult = await PostSettlementVoucherAsync(
            billNumber,
            request,
            totals,
            normalizedPayments,
            settledAtUtc,
            cancellationToken);

        if (voucherResult.IsFailure)
        {
            return Result.Failure<SettledBillReceiptDto>(voucherResult.Error);
        }

        var cashLines = normalizedPayments.Where(p => NormalizeMode(p.Mode) == "Cash").ToList();
        var cashApplied = cashLines.Sum(p => p.Amount);
        var cashReceived = cashLines.Sum(p => p.CashReceived ?? p.Amount);
        var changeToReturn = Math.Max(0m, Math.Round(cashReceived - cashApplied, 2, MidpointRounding.AwayFromZero));

        var creditAmount = normalizedPayments
            .Where(p => NormalizeMode(p.Mode) == "Udhari")
            .Sum(p => p.Amount);

        var prevOutstanding = creditAmount > 0m ? request.CustomerOutstanding : 0m;
        var newOutstanding = Math.Round(prevOutstanding + creditAmount, 2, MidpointRounding.AwayFromZero);

        var primaryMode = ResolvePrimaryMode(request.PaymentMode, normalizedPayments);
        var customerName = string.IsNullOrWhiteSpace(request.CustomerName) ? "Walk-in Customer" : request.CustomerName.Trim();

        var receipt = new SettledBillReceiptDto(
            BillNumber: billNumber,
            IdempotencyKey: idempotencyKey,
            SettledAtUtc: settledAtUtc,
            OrderType: string.IsNullOrWhiteSpace(request.OrderType) ? "Dine-in" : request.OrderType.Trim(),
            TableCode: IsParcelOrder(request.OrderType) ? null : request.TableCode?.Trim(),
            ParcelToken: parcelToken,
            WaiterName: string.IsNullOrWhiteSpace(request.WaiterName) ? "Counter" : request.WaiterName.Trim(),
            CashierName: string.IsNullOrWhiteSpace(cashierName) ? "Cashier" : cashierName.Trim(),
            Items: validItems,
            Totals: totals,
            PrimaryPaymentMode: primaryMode,
            Payments: normalizedPayments,
            CashReceivedTotal: cashReceived,
            ChangeToReturn: changeToReturn,
            CustomerId: request.CustomerId,
            CustomerName: customerName,
            CustomerPhone: request.CustomerPhone?.Trim(),
            CustomerAddress: request.CustomerAddress?.Trim(),
            CreditAmount: creditAmount,
            PreviousOutstanding: prevOutstanding,
            NewOutstanding: newOutstanding,
            VoucherNumber: voucherResult.Value.VoucherNumber,
            IsDuplicate: false);

        lock (_lock)
        {
            _byIdempotencyKey[idempotencyKey] = receipt;
            _byBillNumber[billNumber] = receipt;
            _receipts.Insert(0, receipt);
        }

        return Result.Success(receipt);
    }

    public SettledBillReceiptDto? GetReceipt(string billNumber)
    {
        if (string.IsNullOrWhiteSpace(billNumber))
        {
            return null;
        }

        lock (_lock)
        {
            return _byBillNumber.TryGetValue(billNumber.Trim(), out var receipt) ? receipt : null;
        }
    }

    public IReadOnlyList<SettledBillReceiptDto> GetAllReceipts()
    {
        lock (_lock)
        {
            return _receipts.ToList();
        }
    }

    private async Task<Result<PostedVoucherDto>> PostSettlementVoucherAsync(
        string billNumber,
        SettleBillRequest request,
        BillCalculationResult totals,
        IReadOnlyList<BillPaymentLineInputDto> payments,
        DateTime settledAtUtc,
        CancellationToken cancellationToken)
    {
        var lines = new List<AccountingVoucherLineDto>();

        var cashTotal = payments.Where(p => NormalizeMode(p.Mode) == "Cash").Sum(p => p.Amount);
        var upiTotal = payments.Where(p => NormalizeMode(p.Mode) == "UPI").Sum(p => p.Amount);
        var cardTotal = payments.Where(p => NormalizeMode(p.Mode) == "Card").Sum(p => p.Amount);
        var udhariTotal = payments.Where(p => NormalizeMode(p.Mode) == "Udhari").Sum(p => p.Amount);

        if (cashTotal > 0m)
        {
            lines.Add(new AccountingVoucherLineDto("1110", "Cash in Hand", cashTotal, 0m, $"Bill {billNumber} Cash"));
        }

        if (upiTotal > 0m)
        {
            lines.Add(new AccountingVoucherLineDto("1120", "Bank / UPI Account", upiTotal, 0m, $"Bill {billNumber} UPI"));
        }

        if (cardTotal > 0m)
        {
            lines.Add(new AccountingVoucherLineDto("1121", "Card Settlement Account", cardTotal, 0m, $"Bill {billNumber} Card"));
        }

        if (udhariTotal > 0m)
        {
            var custLabel = string.IsNullOrWhiteSpace(request.CustomerName) ? "Customer" : request.CustomerName.Trim();
            lines.Add(new AccountingVoucherLineDto("1130", "Customer Udhari Debtors", udhariTotal, 0m, $"Bill {billNumber} Udhari - {custLabel}"));
        }

        if (totals.RoundOff < 0m)
        {
            lines.Add(new AccountingVoucherLineDto("5900", "Round Off Expense", Math.Abs(totals.RoundOff), 0m, $"Bill {billNumber} Round Off"));
        }

        var salesAccountCode = IsParcelOrder(request.OrderType) ? "4200" : "4100";
        var salesAccountName = IsParcelOrder(request.OrderType) ? "Parcel & Delivery Sales" : "Dine-In Food Sales";
        if (totals.TaxableAmount > 0m)
        {
            lines.Add(new AccountingVoucherLineDto(salesAccountCode, salesAccountName, 0m, totals.TaxableAmount, $"Bill {billNumber} Sales"));
        }

        if (totals.CgstAmount > 0m)
        {
            lines.Add(new AccountingVoucherLineDto("2121", "CGST Output Payable", 0m, totals.CgstAmount, $"Bill {billNumber} CGST"));
        }

        if (totals.SgstAmount > 0m)
        {
            lines.Add(new AccountingVoucherLineDto("2122", "SGST Output Payable", 0m, totals.SgstAmount, $"Bill {billNumber} SGST"));
        }

        if (totals.RoundOff > 0m)
        {
            lines.Add(new AccountingVoucherLineDto("4900", "Round Off Income", 0m, totals.RoundOff, $"Bill {billNumber} Round Off"));
        }

        var voucherType = udhariTotal > 0m && cashTotal == 0m && upiTotal == 0m && cardTotal == 0m
            ? "Sales (Credit)"
            : "Sales";

        var narration = $"Bill {billNumber} settled ({request.OrderType}) - {ResolvePrimaryMode(request.PaymentMode, payments)}";
        if (request.IsManagerOverride && !string.IsNullOrWhiteSpace(request.ManagerOverrideReason))
        {
            narration += $" [Credit Limit Override: {request.ManagerOverrideReason.Trim()}]";
        }

        var voucherRequest = new PostVoucherRequest(
            VoucherDate: settledAtUtc.Date,
            VoucherType: voucherType,
            Narration: narration,
            ReferenceNumber: billNumber,
            Lines: lines);

        return await _accountingService.PostVoucherAsync(voucherRequest, cancellationToken);
    }

    private static List<BillPaymentLineInputDto> NormalizePayments(SettleBillRequest request, decimal grandTotal)
    {
        if (request.Payments is { Count: > 0 })
        {
            return request.Payments
                .Select(p => p with
                {
                    Mode = NormalizeMode(p.Mode),
                    Amount = Math.Round(p.Amount, 2, MidpointRounding.AwayFromZero),
                    CashReceived = p.CashReceived.HasValue
                        ? Math.Round(p.CashReceived.Value, 2, MidpointRounding.AwayFromZero)
                        : null
                })
                .ToList();
        }

        var mode = NormalizeMode(request.PaymentMode);
        if (string.IsNullOrWhiteSpace(mode) || mode == "Split")
        {
            return [];
        }

        return
        [
            new BillPaymentLineInputDto(
                Mode: mode,
                Amount: grandTotal,
                CashReceived: mode == "Cash" ? grandTotal : null,
                UpiConfirmed: mode == "UPI")
        ];
    }

    private static string NormalizeMode(string? mode)
    {
        var m = (mode ?? string.Empty).Trim();
        if (m.Equals("Credit", StringComparison.OrdinalIgnoreCase) || m.Equals("Udhari", StringComparison.OrdinalIgnoreCase))
        {
            return "Udhari";
        }

        if (m.Equals("UPI", StringComparison.OrdinalIgnoreCase))
        {
            return "UPI";
        }

        if (m.Equals("Card", StringComparison.OrdinalIgnoreCase))
        {
            return "Card";
        }

        if (m.Equals("Split", StringComparison.OrdinalIgnoreCase))
        {
            return "Split";
        }

        return "Cash";
    }

    private static string ResolvePrimaryMode(string? requestedMode, IReadOnlyList<BillPaymentLineInputDto> payments)
    {
        var distinct = payments.Select(p => NormalizeMode(p.Mode)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (distinct.Count > 1)
        {
            return "Split";
        }

        return distinct.FirstOrDefault() ?? NormalizeMode(requestedMode);
    }

    private static bool IsParcelOrder(string? orderType)
    {
        return !string.IsNullOrWhiteSpace(orderType)
               && (orderType.Contains("Parcel", StringComparison.OrdinalIgnoreCase)
                   || orderType.Contains("Takeaway", StringComparison.OrdinalIgnoreCase)
                   || orderType.Contains("Delivery", StringComparison.OrdinalIgnoreCase));
    }
}
