using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class TablesViewModel
{
    public IReadOnlyList<TableItemDto> Tables { get; set; } = new List<TableItemDto>();
    public IReadOnlyList<TableSectionDto> Sections { get; set; } = new List<TableSectionDto>();
    public int TotalTables { get; set; }
    public int OccupiedCount { get; set; }
    public int FreeCount { get; set; }
    public int ReservedCount { get; set; }
    public int CleaningCount { get; set; }
    public decimal TotalRunningSales { get; set; }
    public string SelectedSection { get; set; } = "All";
}
