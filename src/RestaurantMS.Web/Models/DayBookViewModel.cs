using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class DayBookViewModel
{
    public DateTime SelectedDate { get; set; } = DateTime.UtcNow;
    public IReadOnlyList<DayBookEntryDto> Entries { get; set; } = Array.Empty<DayBookEntryDto>();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}
