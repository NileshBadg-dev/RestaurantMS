using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IAuditUiData
{
    Task<PagedResult<AuditLogRowDto>> GetAuditLogsAsync(DataTableRequest request, string? user = null, string? action = null, string? module = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task LogAsync(string userName, string roleName, string action, string module, string details, string ipAddress = "127.0.0.1", string severity = "Info", CancellationToken cancellationToken = default);
}
