using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleAccountsUiData : IAccountsUiData
{
    public Task<IReadOnlyList<AccountNodeDto>> GetChartOfAccountsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AccountNodeDto> tree =
        [
            new("1000", "Assets", "Asset", 485000m,
            [
                new("1100", "Current Assets", "Asset", 285000m,
                [
                    new("1110", "Cash in Hand", "Asset", 35400m, []),
                    new("1120", "HDFC Bank Current A/c", "Asset", 168000m, []),
                    new("1130", "Accounts Receivable (Udhari Debtors)", "Asset", 42600m, []),
                    new("1140", "Closing Inventory", "Asset", 39000m, [])
                ]),
                new("1200", "Fixed Assets", "Asset", 200000m,
                [
                    new("1210", "Kitchen Equipment & Appliances", "Asset", 120000m, []),
                    new("1220", "Furniture & Fixtures", "Asset", 80000m, [])
                ])
            ]),
            new("2000", "Liabilities", "Liability", 145000m,
            [
                new("2100", "Current Liabilities", "Liability", 145000m,
                [
                    new("2110", "Accounts Payable (Suppliers)", "Liability", 85000m, []),
                    new("2120", "GST Payable (Output - Input)", "Liability", 18000m, []),
                    new("2130", "Staff Salaries Payable", "Liability", 42000m, [])
                ])
            ]),
            new("3000", "Equity", "Equity", 200000m,
            [
                new("3100", "Owner Capital", "Equity", 200000m, [])
            ]),
            new("4000", "Income / Revenue", "Revenue", 385000m,
            [
                new("4100", "Dine-in Sales", "Revenue", 265000m, []),
                new("4200", "Parcel Sales", "Revenue", 98000m, []),
                new("4300", "Online Delivery Sales", "Revenue", 22000m, [])
            ]),
            new("5000", "Expenses", "Expense", 245000m,
            [
                new("5100", "Cost of Goods Sold (Ingredients)", "Expense", 140000m, []),
                new("5200", "Staff Wages & Salaries", "Expense", 65000m, []),
                new("5300", "Electricity & Gas Utility", "Expense", 25000m, []),
                new("5400", "Rent & Maintenance", "Expense", 15000m, [])
            ])
        ];
        return Task.FromResult(tree);
    }

    public Task<PagedResult<VoucherItemDto>> GetVouchersAsync(DataTableRequest request, string? voucherType = null, CancellationToken cancellationToken = default)
    {
        var all = new List<VoucherItemDto>
        {
            new(1, "VOU-2026-105", "16/11/2026", "Receipt", "Customer Ramesh Patil (Udhari Recovery)", 0m, 865m),
            new(2, "VOU-2026-104", "16/11/2026", "Payment", "Modern Dairy Suppliers - Butter & Paneer", 4500m, 0m),
            new(3, "VOU-2026-103", "15/11/2026", "Contra", "Cash Deposited to HDFC Bank", 25000m, 25000m),
            new(4, "VOU-2026-102", "15/11/2026", "Journal", "Monthly Electricity Bill Accrual", 12500m, 12500m),
            new(5, "VOU-2026-101", "14/11/2026", "Receipt", "Dinner Sales Settlement Cash", 0m, 18500m)
        };

        if (!string.IsNullOrWhiteSpace(voucherType) && !voucherType.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            all = all.Where(v => v.Type.Equals(voucherType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var paged = all.Skip(request.Start).Take(request.PageSize).ToList();
        return Task.FromResult(new PagedResult<VoucherItemDto>(paged, all.Count, request.PageNumber, request.PageSize));
    }

    public Task<IReadOnlyList<DayBookEntryDto>> GetDayBookAsync(DateTime? date = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DayBookEntryDto> entries =
        [
            new("11:30 am", "VOU-101", "Cash A/c", "Receipt", 18500m, 0m, "Lunch collection"),
            new("02:15 pm", "VOU-102", "Shree Ganesh Grain Mart", "Payment", 0m, 6500m, "Rice supply invoice"),
            new("04:30 pm", "VOU-103", "HDFC Bank Current A/c", "Contra", 10000m, 10000m, "Bank cash deposit"),
            new("08:15 pm", "VOU-104", "Ramesh Patil (Debtor)", "Receipt", 865m, 0m, "Udhari credit repayment"),
            new("10:45 pm", "VOU-105", "Dine-in Sales Revenue", "Sales", 28450m, 0m, "Dinner shift settlement")
        ];
        return Task.FromResult(entries);
    }

    public Task<IReadOnlyList<DayBookEntryDto>> GetCashBookAsync(DateTime? date = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DayBookEntryDto> entries =
        [
            new("10:00 am", "CB-01", "Opening Cash Balance", "Opening", 15000m, 0m, "Cash in drawer"),
            new("02:00 pm", "CB-02", "Vegetable Purchase", "Payment", 0m, 1200m, "Fresh coriander and green chilies"),
            new("04:30 pm", "CB-03", "Cash Deposited to Bank", "Contra", 0m, 10000m, "Excess cash deposit"),
            new("09:00 pm", "CB-04", "Lunch & Dinner Cash Sales", "Receipt", 31600m, 0m, "Daily cash collections")
        ];
        return Task.FromResult(entries);
    }

    public Task<FinancialStatementDto> GetTrialBalanceAsync(DateTime? asOf = null, CancellationToken cancellationToken = default)
    {
        var debits = new List<FinancialStatementLineDto>
        {
            new("Cash in Hand", 35400m),
            new("HDFC Bank Current A/c", 168000m),
            new("Sundry Debtors (Udhari)", 42600m),
            new("Kitchen Equipment", 120000m),
            new("Cost of Goods Sold", 140000m),
            new("Staff Salaries", 65000m),
            new("Rent & Electricity", 40000m)
        };

        var credits = new List<FinancialStatementLineDto>
        {
            new("Owner Capital", 200000m),
            new("Sundry Creditors (Suppliers)", 85000m),
            new("GST Output Liability", 18000m),
            new("Dine-in Sales", 265000m),
            new("Parcel Sales", 43000m)
        };

        var stmt = new FinancialStatementDto(
            Title: "Trial Balance",
            PeriodOrAsOf: "As of 16/11/2026",
            LeftLines: debits,
            RightLines: credits,
            TotalLeft: 611000m,
            TotalRight: 611000m);

        return Task.FromResult(stmt);
    }

    public Task<FinancialStatementDto> GetProfitLossAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var expenses = new List<FinancialStatementLineDto>
        {
            new("Cost of Materials (Food & Beverage)", 140000m),
            new("Employee Benefit Expenses", 65000m),
            new("Power & Fuel Utility", 25000m),
            new("Rent, Rates & Taxes", 15000m),
            new("Net Profit before Tax", 140000m, false, true)
        };

        var incomes = new List<FinancialStatementLineDto>
        {
            new("Revenue from Dine-in Operations", 265000m),
            new("Revenue from Takeaway Parcels", 98000m),
            new("Online Deliveries", 22000m)
        };

        var stmt = new FinancialStatementDto(
            Title: "Profit & Loss Account",
            PeriodOrAsOf: "01/11/2026 to 16/11/2026",
            LeftLines: expenses,
            RightLines: incomes,
            TotalLeft: 385000m,
            TotalRight: 385000m);

        return Task.FromResult(stmt);
    }

    public Task<FinancialStatementDto> GetBalanceSheetAsync(DateTime asOf, CancellationToken cancellationToken = default)
    {
        var assets = new List<FinancialStatementLineDto>
        {
            new("Fixed Assets (Machinery & Furnishings)", 200000m),
            new("Current Assets - Inventories", 39000m),
            new("Trade Receivables (Customer Credit)", 42600m),
            new("Cash and Bank Balances", 203400m)
        };

        var liabilities = new List<FinancialStatementLineDto>
        {
            new("Proprietor Capital Account", 200000m),
            new("Retained Earnings / Current Period Profit", 140000m),
            new("Trade Payables (Suppliers)", 85000m),
            new("Other Current Liabilities & Taxes", 60000m)
        };

        var stmt = new FinancialStatementDto(
            Title: "Balance Sheet",
            PeriodOrAsOf: "As of 16/11/2026",
            LeftLines: liabilities,
            RightLines: assets,
            TotalLeft: 485000m,
            TotalRight: 485000m);

        return Task.FromResult(stmt);
    }

    public Task<long> CreateVoucherAsync(string voucherType, decimal totalAmount, string? narration, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(106L);
    }
}
