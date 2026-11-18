using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleBillingUiData : IBillingUiData
{
    private static readonly List<BillSummaryRowDto> SettledBills = [];
    private static int _billSeq = 1001;
    private static readonly object _lock = new();

    public Task<BillingOrderDetailsDto?> GetBillForTableAsync(string tableCode, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var code = string.IsNullOrWhiteSpace(tableCode) ? "T1" : tableCode;
            var items = SampleTablesUiData.GetItemsForTable(code);

            var subtotal = items.Sum(i => i.Amount);
            var gst = Math.Round(subtotal * 0.05m, 2);
            var total = Math.Round(subtotal + gst);

            var bill = new BillingOrderDetailsDto(
                OrderType: "Dine-in",
                TableCode: code,
                Subtotal: subtotal,
                GstPercent: 5.0m,
                GstAmount: gst,
                TotalAmount: total,
                SuggestedCustomerName: "Walk-in Customer",
                SuggestedCustomerMobile: "",
                OutstandingCredit: 0m,
                CreditLimit: 10000m,
                Items: items);

            return Task.FromResult<BillingOrderDetailsDto?>(bill);
        }
    }

    public Task<PagedResult<BillSummaryRowDto>> GetBillsListAsync(DataTableRequest request, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var all = SettledBills.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                var search = request.SearchValue.Trim().ToLowerInvariant();
                all = all.Where(b => b.BillNo.ToLowerInvariant().Contains(search) ||
                                     b.TableOrCustomer.ToLowerInvariant().Contains(search) ||
                                     b.PaymentMode.ToLowerInvariant().Contains(search));
            }

            var list = all.ToList();
            var paged = list.Skip(request.Start).Take(request.PageSize).ToList();
            return Task.FromResult(new PagedResult<BillSummaryRowDto>(paged, list.Count, request.PageNumber, request.PageSize));
        }
    }

    public static string RecordSettledBill(string orderType, string tableOrCustomer, decimal amount, string paymentMode)
    {
        lock (_lock)
        {
            var billNo = $"B-{_billSeq++}";
            return RecordSettledBillWithNumber(billNo, orderType, tableOrCustomer, amount, paymentMode);
        }
    }

    public static string RecordSettledBillWithNumber(string billNo, string orderType, string tableOrCustomer, decimal amount, string paymentMode)
    {
        lock (_lock)
        {
            if (SettledBills.Any(b => b.BillNo.Equals(billNo, StringComparison.OrdinalIgnoreCase)))
            {
                return billNo;
            }

            var timeFormatted = DateTime.UtcNow.AddHours(5).AddMinutes(30).ToString("dd-MMM, h:mm tt");
            var bill = new BillSummaryRowDto(billNo, timeFormatted, orderType, tableOrCustomer, amount, paymentMode, "Settled");
            SettledBills.Insert(0, bill);

            // Free the table if Dine-in
            if (tableOrCustomer.StartsWith("Table", StringComparison.OrdinalIgnoreCase) || tableOrCustomer.StartsWith("T", StringComparison.OrdinalIgnoreCase))
            {
                var code = tableOrCustomer.Replace("Table", "").Trim();
                SampleTablesUiData.ReleaseTable(code);
                SampleOrdersUiData.CloseOrder(code);
            }

            return billNo;
        }
    }

    public static IReadOnlyList<BillSummaryRowDto> GetAllSettledBills()
    {
        lock (_lock)
        {
            return SettledBills.ToList();
        }
    }
}
