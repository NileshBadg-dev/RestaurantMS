using System.Text;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Reports;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Reports.Services;

public class ReportsService : IReportsService
{
    private readonly ReportsStateStore _stateStore;
    private readonly ReportDataGenerator _generator;
    private readonly IDateTime _dateTime;

    public ReportsService(ReportsStateStore stateStore, ReportDataGenerator generator, IDateTime dateTime)
    {
        _stateStore = stateStore;
        _generator = generator;
        _dateTime = dateTime;
    }

    public IReadOnlyList<string> GetCategories() => ReportsCatalog.Categories;

    public IReadOnlyList<ReportDefinitionDto> GetCatalog(IReadOnlyCollection<string> userPermissions, bool isSuperUser)
    {
        return ReportsCatalog.Definitions
            .Where(d => HasReportPermission(d, userPermissions, isSuperUser))
            .ToList();
    }

    public ReportDefinitionDto? GetDefinition(string reportCode)
    {
        if (string.IsNullOrWhiteSpace(reportCode)) return null;
        return ReportsCatalog.Definitions.FirstOrDefault(d =>
            d.Code.Equals(reportCode.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public bool HasReportPermission(ReportDefinitionDto report, IReadOnlyCollection<string> userPermissions, bool isSuperUser)
    {
        if (isSuperUser) return true;
        if (userPermissions.Count == 0) return false;
        return userPermissions.Contains(report.RequiredPermission, StringComparer.OrdinalIgnoreCase);
    }

    public UserReportPreferencesDto GetUserPreferences(string userId) => _stateStore.GetPreferences(userId);

    public bool ToggleFavorite(string userId, string reportCode) => _stateStore.ToggleFavorite(userId, reportCode);

    public void RecordRecentlyUsed(string userId, string reportCode) => _stateStore.RecordRecentlyUsed(userId, reportCode);

    public void SaveUserFilter(string userId, string reportCode, ReportFilterModel filter) =>
        _stateStore.SaveFilter(userId, reportCode, filter.Normalized(CurrentIstDate()));

    public ReportFilterModel? GetSavedUserFilter(string userId, string reportCode) =>
        _stateStore.GetSavedFilter(userId, reportCode);

    public void ClearSavedUserFilter(string userId, string reportCode) =>
        _stateStore.ClearSavedFilter(userId, reportCode);

    public Result<ReportExecutionResultDto> ExecuteReport(string reportCode, ReportFilterModel filter)
    {
        var def = GetDefinition(reportCode);
        if (def is null)
        {
            return Result.Failure<ReportExecutionResultDto>("Reports.NotFound", $"Report '{reportCode}' was not found.");
        }

        var today = CurrentIstDate();
        var normalized = (filter ?? new ReportFilterModel()).Normalized(today);
        normalized.ReportCode = def.Code;

        var payload = _generator.Build(def, normalized);
        var totalRows = payload.Rows.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalRows / (double)normalized.PageSize));
        var pageNumber = Math.Clamp(normalized.PageNumber, 1, totalPages);
        normalized.PageNumber = pageNumber;

        var pagedRows = payload.Rows
            .Skip((pageNumber - 1) * normalized.PageSize)
            .Take(normalized.PageSize)
            .Select(r => new ReportGridRowDto(
                r.Cells,
                r.GroupKey,
                r.DrillDownKey,
                r.DrillDownType,
                r.DrillDownTitle,
                r.DrillDownDetails))
            .ToList();

        var result = new ReportExecutionResultDto(
            Report: def,
            AppliedFilter: normalized,
            ActiveFilterSummary: normalized.GetActiveFilterSummary(today),
            Kpis: payload.Kpis,
            Chart: def.SupportsChart ? payload.Chart : null,
            Headers: payload.Headers,
            Rows: pagedRows,
            TotalsRow: payload.TotalsRow,
            TotalRows: totalRows,
            PageNumber: pageNumber,
            PageSize: normalized.PageSize,
            TotalPages: totalPages,
            GroupByLabel: payload.GroupByLabel,
            GeneratedAtUtc: _dateTime.UtcNow);

        return Result.Success(result);
    }

    public Result<string> ExportCsv(string reportCode, ReportFilterModel filter, string hotelName)
    {
        var exportFilter = (filter ?? new ReportFilterModel()).Normalized(CurrentIstDate());
        exportFilter.PageNumber = 1;
        exportFilter.PageSize = 100;

        var exec = ExecuteReport(reportCode, exportFilter);
        if (exec.IsFailure) return Result.Failure<string>(exec.Error);

        var data = exec.Value;
        var sb = new StringBuilder();
        sb.AppendLine(CsvRow([hotelName, data.Report.TitleEn]));
        sb.AppendLine(CsvRow(["Filters", string.Join(" | ", data.ActiveFilterSummary)]));
        sb.AppendLine();
        sb.AppendLine(CsvRow(data.Headers));

        foreach (var row in data.Rows)
        {
            sb.AppendLine(CsvRow(row.Cells));
        }

        if (data.TotalsRow.Count > 0)
        {
            sb.AppendLine(CsvRow(data.TotalsRow));
        }

        return Result.Success(sb.ToString());
    }

    private static string CsvRow(IEnumerable<string> cells)
    {
        return string.Join(",", cells.Select(c =>
        {
            var s = (c ?? string.Empty).Replace("\"", "\"\"");
            return $"\"{s}\"";
        }));
    }

    private DateTime CurrentIstDate() => _dateTime.UtcNow.AddHours(5).AddMinutes(30).Date;
}
