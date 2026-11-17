namespace RestaurantMS.Web.Models;

public class ReportFilterBarModel
{
    public bool ShowDateRange { get; set; } = true;
    public bool ShowShift { get; set; } = true;
    public bool ShowWaiter { get; set; } = false;
    public bool ShowTable { get; set; } = false;
    public bool ShowCategory { get; set; } = false;
    public bool ShowPaymentMode { get; set; } = false;

    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? SelectedShift { get; set; }
    public string? SelectedWaiter { get; set; }
    public string? SelectedTable { get; set; }
    public string? SelectedCategory { get; set; }
    public string? SelectedPaymentMode { get; set; }
}
