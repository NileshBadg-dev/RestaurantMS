using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class ReportDetailViewModel
{
    public string ReportCode { get; set; } = string.Empty;
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime FromDate { get; set; } = DateTime.UtcNow.AddDays(-7);
    public DateTime ToDate { get; set; } = DateTime.UtcNow;
    public string Shift { get; set; } = "All";
    public string Waiter { get; set; } = "All";
    public string Table { get; set; } = "All";
    public string Category { get; set; } = "All";
    public string PaymentMode { get; set; } = "All";
    public ReportResultDto Result { get; set; } = new(string.Empty, string.Empty, string.Empty, Array.Empty<string>(), Array.Empty<IReadOnlyList<string>>(), null);
}
