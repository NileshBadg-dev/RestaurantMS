using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class AccountsController : Controller
{
    private readonly IAccountsUiData _accountsUiData;

    public AccountsController(IAccountsUiData accountsUiData)
    {
        _accountsUiData = accountsUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var rootAccounts = await _accountsUiData.GetChartOfAccountsAsync(cancellationToken);

        var viewModel = new ChartOfAccountsViewModel
        {
            RootAccounts = rootAccounts,
            TotalAssets = 1450000,
            TotalLiabilities = 320000,
            TotalEquity = 1130000
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Vouchers([FromQuery] string? type, CancellationToken cancellationToken)
    {
        var vouchers = await _accountsUiData.GetVouchersAsync(new DataTableRequest { Length = 100 }, type, cancellationToken);

        var viewModel = new VouchersListViewModel
        {
            Vouchers = vouchers.Items,
            SelectedType = type ?? "All",
            TotalDebits = vouchers.Items.Sum(v => v.DebitAmount),
            TotalCredits = vouchers.Items.Sum(v => v.CreditAmount)
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult CreateVoucher()
    {
        var viewModel = new VoucherFormViewModel
        {
            VoucherNumber = "VCH-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-01",
            Lines = new List<VoucherEntryLineModel>
            {
                new() { AccountName = "Vegetable Purchase A/c", Debit = 2500, Credit = 0, Narration = "Cash purchase" },
                new() { AccountName = "Cash in Hand A/c", Debit = 0, Credit = 2500, Narration = "Paid cash" }
            },
            TotalDebit = 2500,
            TotalCredit = 2500
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVoucher(VoucherFormViewModel model, string? submitAction = null, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid || model.TotalDebit != model.TotalCredit)
        {
            ModelState.AddModelError(string.Empty, "Double-entry must balance: Total Debits must equal Total Credits.");
            return View(model);
        }

        var vchId = await _accountsUiData.CreateVoucherAsync(model.VoucherType, model.TotalDebit, model.Narration, cancellationToken);
        TempData["SuccessMessage"] = $"Voucher #{vchId} recorded successfully!";
        TempData["HighlightedRow"] = model.VoucherNumber;
        if (string.Equals(submitAction, "saveAndNew", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CreateVoucher));
        }

        return RedirectToAction(nameof(Vouchers));
    }

    [HttpGet]
    public async Task<IActionResult> DayBook([FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        var selectedDate = date ?? DateTime.UtcNow;
        var entries = await _accountsUiData.GetDayBookAsync(selectedDate, cancellationToken);

        var viewModel = new DayBookViewModel
        {
            SelectedDate = selectedDate,
            Entries = entries,
            TotalDebit = entries.Sum(e => e.DebitAmount),
            TotalCredit = entries.Sum(e => e.CreditAmount)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> CashBook([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to ?? DateTime.UtcNow;
        var transactions = await _accountsUiData.GetCashBookAsync(fromDate, cancellationToken);

        var viewModel = new CashBookViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = 25000,
            TotalReceipts = transactions.Sum(t => t.DebitAmount),
            TotalPayments = transactions.Sum(t => t.CreditAmount),
            ClosingBalance = 25000 + transactions.Sum(t => t.DebitAmount) - transactions.Sum(t => t.CreditAmount),
            Transactions = transactions
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> BankBook([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to ?? DateTime.UtcNow;
        var transactions = await _accountsUiData.GetCashBookAsync(fromDate, cancellationToken);

        var viewModel = new CashBookViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = 150000,
            TotalReceipts = transactions.Sum(t => t.DebitAmount),
            TotalPayments = transactions.Sum(t => t.CreditAmount),
            ClosingBalance = 150000 + transactions.Sum(t => t.DebitAmount) - transactions.Sum(t => t.CreditAmount),
            Transactions = transactions
        };

        ViewData["Title"] = "Bank Book (HDFC Bank & UPI Settlement)";
        return View("CashBook", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> TrialBalance([FromQuery] DateTime? asOf, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var targetDate = asOf ?? to ?? DateTime.UtcNow;
        var statement = await _accountsUiData.GetTrialBalanceAsync(targetDate, cancellationToken);

        var viewModel = new FinancialReportViewModel
        {
            ReportType = "TrialBalance",
            AsOfDate = targetDate,
            Statement = statement
        };

        return View("Statement", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ProfitLoss([FromQuery] DateTime? asOf, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var fromDate = from ?? asOf?.AddMonths(-1) ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? asOf ?? DateTime.UtcNow;
        var statement = await _accountsUiData.GetProfitLossAsync(fromDate, toDate, cancellationToken);

        var viewModel = new FinancialReportViewModel
        {
            ReportType = "ProfitLoss",
            AsOfDate = toDate,
            Statement = statement
        };

        return View("Statement", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> BalanceSheet([FromQuery] DateTime? asOf, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var targetDate = asOf ?? to ?? DateTime.UtcNow;
        var statement = await _accountsUiData.GetBalanceSheetAsync(targetDate, cancellationToken);

        var viewModel = new FinancialReportViewModel
        {
            ReportType = "BalanceSheet",
            AsOfDate = targetDate,
            Statement = statement
        };

        return View("Statement", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> LedgerSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? accountName, CancellationToken cancellationToken)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow;
        var transactions = await _accountsUiData.GetCashBookAsync(fromDate, cancellationToken);

        var viewModel = new CashBookViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = 45000,
            TotalReceipts = transactions.Sum(t => t.DebitAmount),
            TotalPayments = transactions.Sum(t => t.CreditAmount),
            ClosingBalance = 45000 + transactions.Sum(t => t.DebitAmount) - transactions.Sum(t => t.CreditAmount),
            Transactions = transactions
        };

        ViewData["Title"] = $"General Ledger Summary: {accountName ?? "All Primary Ledgers"}";
        return View("CashBook", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> AuditReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        var trial = await _accountsUiData.GetTrialBalanceAsync(toDate, cancellationToken);
        var pl = await _accountsUiData.GetProfitLossAsync(fromDate, toDate, cancellationToken);
        var bs = await _accountsUiData.GetBalanceSheetAsync(toDate, cancellationToken);

        var viewModel = new AccountsAuditReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            TrialBalance = trial,
            ProfitLoss = pl,
            BalanceSheet = bs,
            DoubleEntryBalanced = Math.Abs(trial.TotalLeft - trial.TotalRight) < 0.01m,
            BalanceSheetBalanced = Math.Abs(bs.TotalLeft - bs.TotalRight) < 0.01m,
            UnpostedVouchersCount = 0,
            UdhariDiscrepanciesCount = 0
        };

        return View(viewModel);
    }
}
