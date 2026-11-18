namespace RestaurantMS.Web.Models;

public class ReportFilterBarModel
{
    public string ReportCode { get; set; } = "RPT01";
    public string DatePreset { get; set; } = "ThisMonth";
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string SelectedShift { get; set; } = "All";
    public string SelectedWaiter { get; set; } = "All";
    public string SelectedTable { get; set; } = "All";
    public string SelectedCategory { get; set; } = "All";
    public string SelectedPaymentMode { get; set; } = "All";
    public string SelectedOrderType { get; set; } = "All";
    public string? Search { get; set; }
    public int PageSize { get; set; } = 25;
    public bool ShowDateRange { get; set; } = true;
    public bool ShowShift { get; set; } = true;
    public bool ShowWaiter { get; set; } = true;
    public bool ShowTable { get; set; } = true;
    public bool ShowCategory { get; set; } = true;
    public bool ShowPaymentMode { get; set; } = true;
    public bool ShowOrderType { get; set; } = true;
}
