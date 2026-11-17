using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class ReportsHubViewModel
{
    public IReadOnlyList<ReportHubCardDto> Reports { get; set; } = Array.Empty<ReportHubCardDto>();
    public string SelectedCategory { get; set; } = "All";
}
