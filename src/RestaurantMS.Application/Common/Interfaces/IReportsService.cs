using RestaurantMS.Application.Common.Models.Reports;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Common.Interfaces;

/// <summary>
/// Central reporting catalog, per-user preference/filter persistence, execution, paging and export service.
/// </summary>
public interface IReportsService
{
    IReadOnlyList<string> GetCategories();

    IReadOnlyList<ReportDefinitionDto> GetCatalog(IReadOnlyCollection<string> userPermissions, bool isSuperUser);

    ReportDefinitionDto? GetDefinition(string reportCode);

    bool HasReportPermission(ReportDefinitionDto report, IReadOnlyCollection<string> userPermissions, bool isSuperUser);

    UserReportPreferencesDto GetUserPreferences(string userId);

    bool ToggleFavorite(string userId, string reportCode);

    void RecordRecentlyUsed(string userId, string reportCode);

    void SaveUserFilter(string userId, string reportCode, ReportFilterModel filter);

    ReportFilterModel? GetSavedUserFilter(string userId, string reportCode);

    void ClearSavedUserFilter(string userId, string reportCode);

    Result<ReportExecutionResultDto> ExecuteReport(string reportCode, ReportFilterModel filter);

    Result<string> ExportCsv(string reportCode, ReportFilterModel filter, string hotelName);
}
