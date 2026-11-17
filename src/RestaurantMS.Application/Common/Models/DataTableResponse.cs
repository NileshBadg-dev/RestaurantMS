namespace RestaurantMS.Application.Common.Models;

public class DataTableResponse<T>
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public IReadOnlyList<T> Data { get; set; } = [];

    public static DataTableResponse<T> FromPagedResult(PagedResult<T> pagedResult, int draw)
    {
        return new DataTableResponse<T>
        {
            Draw = draw,
            RecordsTotal = pagedResult.TotalCount,
            RecordsFiltered = pagedResult.TotalCount,
            Data = pagedResult.Items
        };
    }
}
