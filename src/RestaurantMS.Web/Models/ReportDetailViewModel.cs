using RestaurantMS.Application.Common.Models.Reports;

namespace RestaurantMS.Web.Models;

public class ReportDetailViewModel
{
    public string ReportCode { get; set; } = string.Empty;
    public string ReportTitle { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
    public bool CanExport { get; set; } = true;
    public bool CanPrint { get; set; } = true;
    public string HotelName { get; set; } = "Grand Restaurant";
    public string? HotelAddress { get; set; }
    public string? HotelGstin { get; set; }
    public string? HotelLogoUrl { get; set; }
    public ReportExecutionResultDto Execution { get; set; } = null!;
}
