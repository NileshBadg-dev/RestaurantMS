namespace RestaurantMS.Application.Common.Models.Reports;

public record ReportChartDto(
    string ChartType,
    string Title,
    IReadOnlyList<string> Labels,
    string SeriesLabel,
    IReadOnlyList<decimal> Values);
