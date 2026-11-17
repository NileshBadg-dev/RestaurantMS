namespace RestaurantMS.Web.Models;

public class DataGridColumn
{
    public string Data { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool Sortable { get; set; } = true;
    public bool Filterable { get; set; } = false;
    public string? Width { get; set; }
    public string? CustomClass { get; set; }

    public DataGridColumn() { }

    public DataGridColumn(string data, string title, bool sortable = true, bool filterable = false, string? width = null, string? customClass = null)
    {
        Data = data;
        Title = title;
        Sortable = sortable;
        Filterable = filterable;
        Width = width;
        CustomClass = customClass;
    }
}
