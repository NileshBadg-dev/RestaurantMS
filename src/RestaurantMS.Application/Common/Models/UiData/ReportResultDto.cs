namespace RestaurantMS.Application.Common.Models.UiData;

public record ReportResultDto(
    string ReportKey,
    string Title,
    string Period,
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    IReadOnlyDictionary<string, string>? Totals);
