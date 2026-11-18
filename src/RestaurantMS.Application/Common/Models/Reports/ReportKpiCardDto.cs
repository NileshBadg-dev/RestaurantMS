namespace RestaurantMS.Application.Common.Models.Reports;

public record ReportKpiCardDto(
    string Label,
    string Value,
    string Subtitle,
    string Tone,
    string Icon);
