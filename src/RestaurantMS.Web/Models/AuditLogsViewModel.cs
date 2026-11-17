using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class AuditLogsViewModel
{
    public IReadOnlyList<AuditLogRowDto> Logs { get; set; } = [];
    public string? SelectedUser { get; set; }
    public string? SelectedAction { get; set; }
    public string? SelectedModule { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int TotalLogsCount { get; set; }
}
