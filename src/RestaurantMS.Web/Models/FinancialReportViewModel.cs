using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class FinancialReportViewModel
{
    public string ReportType { get; set; } = "TrialBalance"; // TrialBalance, ProfitLoss, BalanceSheet
    public DateTime AsOfDate { get; set; } = DateTime.UtcNow;
    public FinancialStatementDto Statement { get; set; } = new(string.Empty, string.Empty, Array.Empty<FinancialStatementLineDto>(), Array.Empty<FinancialStatementLineDto>(), 0m, 0m);
}
