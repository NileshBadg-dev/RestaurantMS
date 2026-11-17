namespace RestaurantMS.Application.Common.Models;

public class DataTableRequest
{
    public int Draw { get; set; } = 1;
    public int Start { get; set; } = 0;
    public int Length { get; set; } = 10;
    public string? SearchValue { get; set; }
    public string? SortColumn { get; set; }
    public string SortDirection { get; set; } = "asc";
    public Dictionary<string, string> ColumnFilters { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public int PageNumber => Length > 0 ? (Start / Length) + 1 : 1;
    public int PageSize => Length > 0 ? Length : 10;
}
