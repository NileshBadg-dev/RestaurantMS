using RestaurantMS.Application.Common.Models.Reports;

namespace RestaurantMS.Web.Models;

public class ReportsHubViewModel
{
    public IReadOnlyList<string> Categories { get; set; } = [];
    public IReadOnlyList<ReportDefinitionDto> AllReports { get; set; } = [];
    public IReadOnlyList<ReportDefinitionDto> FavoriteReports { get; set; } = [];
    public IReadOnlyList<ReportDefinitionDto> RecentReports { get; set; } = [];
    public IReadOnlySet<string> FavoriteCodes { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public string SelectedCategory { get; set; } = "All";
    public string? Search { get; set; }
}
