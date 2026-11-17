namespace RestaurantMS.Web.Models;

public class DataGridModel
{
    public string TableId { get; set; } = "appDataGrid";
    public string? AjaxUrl { get; set; }
    public IReadOnlyList<DataGridColumn> Columns { get; set; } = [];
    public bool ExportButtons { get; set; } = true;
    public bool DensityToggle { get; set; } = true;
    public bool ShowTotals { get; set; } = false;
    public string? CreateUrl { get; set; }
    public string? CreatePermission { get; set; }
    public string? CreateButtonText { get; set; }
    public string? EmptyText { get; set; }
}
