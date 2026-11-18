namespace RestaurantMS.Application.Common.Models.Reports;

public record ReportDefinitionDto(
    string Code,
    string TitleEn,
    string TitleMr,
    string DescriptionEn,
    string DescriptionMr,
    string Category,
    string Icon,
    string RequiredPermission,
    bool SupportsChart,
    string DefaultChartType,
    string? DefaultGroupBy,
    string DrillDownType);
