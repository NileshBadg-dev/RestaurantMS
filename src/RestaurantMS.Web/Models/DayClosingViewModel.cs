using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class DayClosingViewModel
{
    public DayClosingSummaryDto Summary { get; set; } = new(DateTime.UtcNow.ToString("dd/MM/yyyy"), 0, 0, 0, 0, 0, 0, 0, 0, false);
    public decimal ActualCashInDrawer { get; set; }
    public decimal Discrepancy => ActualCashInDrawer - Summary.ExpectedDrawerCash;
    public string ClosingRemarks { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
}
