namespace RestaurantMS.Application.Common.Models.UiData;

public record TableTimeStatsDto(
    int AverageMinutes,
    int ShortestMinutes,
    int LongestMinutes,
    IReadOnlyDictionary<string, int> SectionAverages);
