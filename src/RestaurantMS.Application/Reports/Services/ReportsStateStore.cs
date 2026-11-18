using RestaurantMS.Application.Common.Models.Reports;

namespace RestaurantMS.Application.Reports.Services;

/// <summary>
/// Thread-safe singleton store for per-user report favorites, recently used list, and persisted filters.
/// </summary>
public class ReportsStateStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, HashSet<string>> _favoritesByUser = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> _recentByUser = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ReportFilterModel> _filtersByUserReport = new(StringComparer.OrdinalIgnoreCase);

    public UserReportPreferencesDto GetPreferences(string userId)
    {
        var key = NormalizeUser(userId);
        lock (_lock)
        {
            var favs = _favoritesByUser.TryGetValue(key, out var f) ? f.ToList() : ["RPT01", "RPT07", "RPT19"];
            var rec = _recentByUser.TryGetValue(key, out var r) ? r.ToList() : [];
            return new UserReportPreferencesDto(favs, rec);
        }
    }

    public bool ToggleFavorite(string userId, string reportCode)
    {
        var u = NormalizeUser(userId);
        var code = reportCode.Trim().ToUpperInvariant();
        lock (_lock)
        {
            if (!_favoritesByUser.TryGetValue(u, out var set))
            {
                set = new HashSet<string>(["RPT01", "RPT07", "RPT19"], StringComparer.OrdinalIgnoreCase);
                _favoritesByUser[u] = set;
            }

            if (!set.Add(code))
            {
                set.Remove(code);
                return false;
            }

            return true;
        }
    }

    public void RecordRecentlyUsed(string userId, string reportCode)
    {
        var u = NormalizeUser(userId);
        var code = reportCode.Trim().ToUpperInvariant();
        lock (_lock)
        {
            if (!_recentByUser.TryGetValue(u, out var list))
            {
                list = [];
                _recentByUser[u] = list;
            }

            list.RemoveAll(c => c.Equals(code, StringComparison.OrdinalIgnoreCase));
            list.Insert(0, code);
            if (list.Count > 6)
            {
                list.RemoveRange(6, list.Count - 6);
            }
        }
    }

    public void SaveFilter(string userId, string reportCode, ReportFilterModel filter)
    {
        var key = FilterKey(userId, reportCode);
        lock (_lock)
        {
            _filtersByUserReport[key] = filter;
        }
    }

    public ReportFilterModel? GetSavedFilter(string userId, string reportCode)
    {
        var key = FilterKey(userId, reportCode);
        lock (_lock)
        {
            return _filtersByUserReport.TryGetValue(key, out var f) ? f : null;
        }
    }

    public void ClearSavedFilter(string userId, string reportCode)
    {
        var key = FilterKey(userId, reportCode);
        lock (_lock)
        {
            _filtersByUserReport.Remove(key);
        }
    }

    private static string NormalizeUser(string? userId) =>
        string.IsNullOrWhiteSpace(userId) ? "anonymous" : userId.Trim();

    private static string FilterKey(string userId, string reportCode) =>
        $"{NormalizeUser(userId)}::{reportCode.Trim().ToUpperInvariant()}";
}
