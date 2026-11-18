namespace RestaurantMS.Application.Common.Models.Reports;

public record UserReportPreferencesDto(
    IReadOnlyList<string> FavoriteCodes,
    IReadOnlyList<string> RecentCodes);
