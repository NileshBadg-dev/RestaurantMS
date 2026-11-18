namespace RestaurantMS.Application.Common.Models.Reports;

public record ReportExecutionResultDto(
    ReportDefinitionDto Report,
    ReportFilterModel AppliedFilter,
    IReadOnlyList<string> ActiveFilterSummary,
    IReadOnlyList<ReportKpiCardDto> Kpis,
    ReportChartDto? Chart,
    IReadOnlyList<string> Headers,
    IReadOnlyList<ReportGridRowDto> Rows,
    IReadOnlyList<string> TotalsRow,
    int TotalRows,
    int PageNumber,
    int PageSize,
    int TotalPages,
    string? GroupByLabel,
    DateTime GeneratedAtUtc);
