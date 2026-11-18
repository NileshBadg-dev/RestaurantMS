namespace RestaurantMS.Application.Common.Models.Reports;

public record ReportGridRowDto(
    IReadOnlyList<string> Cells,
    string? GroupKey,
    string? DrillDownKey,
    string? DrillDownType,
    string? DrillDownTitle,
    IReadOnlyDictionary<string, string> DrillDownDetails);
