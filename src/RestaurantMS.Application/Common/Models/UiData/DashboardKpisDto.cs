namespace RestaurantMS.Application.Common.Models.UiData;

public record DashboardKpisDto(
    decimal TodaySales,
    decimal YesterdayComparisonPercent,
    int OrdersDone,
    int DineInOrdersDone,
    decimal DineInSales,
    int ParcelsDone,
    decimal ParcelSales,
    int PendingParcelPickups,
    int ActiveTables,
    int TotalTables,
    int FreeTables,
    int ReservedTables,
    int AvgTableMinutes,
    string LongestTableInfo);
