using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleAuditUiData : IAuditUiData
{
    private static readonly List<AuditLogRowDto> AuditLogs =
    [
        new(1, DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss"), "admin", "Owner", "System Initialized", "System", "Phase 2 Interactive POS Initialized with zero dummy orders", "127.0.0.1", "Info"),
        new(2, DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss"), "admin", "Owner", "User Login", "Auth", "Administrator signed into POS Terminal 1", "127.0.0.1", "Info")
    ];

    private static long _nextId = 3;
    private static readonly object _lock = new();

    public Task<PagedResult<AuditLogRowDto>> GetAuditLogsAsync(
        DataTableRequest request,
        string? user = null,
        string? action = null,
        string? module = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = AuditLogs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(user) && !user.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(l => l.UserName.Equals(user, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(action) && !action.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(l => l.Action.Contains(action, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(module) && !module.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(l => l.Module.Equals(module, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                var s = request.SearchValue.Trim().ToLowerInvariant();
                query = query.Where(l =>
                    l.UserName.ToLowerInvariant().Contains(s) ||
                    l.Action.ToLowerInvariant().Contains(s) ||
                    l.Details.ToLowerInvariant().Contains(s) ||
                    l.Module.ToLowerInvariant().Contains(s));
            }

            var list = query.OrderByDescending(l => l.Id).ToList();
            var paged = list.Skip(request.Start).Take(request.PageSize).ToList();

            return Task.FromResult(new PagedResult<AuditLogRowDto>(paged, list.Count, request.PageNumber, request.PageSize));
        }
    }

    public Task LogAsync(
        string userName,
        string roleName,
        string action,
        string module,
        string details,
        string ipAddress = "127.0.0.1",
        string severity = "Info",
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var id = _nextId++;
            var timestamp = DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss");
            var entry = new AuditLogRowDto(id, timestamp, userName, roleName, action, module, details, ipAddress, severity);
            AuditLogs.Insert(0, entry);
        }

        return Task.CompletedTask;
    }
}
