using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class AccountsAuditReportViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public FinancialStatementDto TrialBalance { get; set; } = new(string.Empty, string.Empty, [], [], 0, 0);
    public FinancialStatementDto ProfitLoss { get; set; } = new(string.Empty, string.Empty, [], [], 0, 0);
    public FinancialStatementDto BalanceSheet { get; set; } = new(string.Empty, string.Empty, [], [], 0, 0);
    public bool DoubleEntryBalanced { get; set; }
    public bool BalanceSheetBalanced { get; set; }
    public int UnpostedVouchersCount { get; set; }
    public int UdhariDiscrepanciesCount { get; set; }
}
