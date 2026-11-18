namespace RestaurantMS.Application.Common.Models.Staff;

public record MonthlyCalendarDayDto(
    DateTime Date,
    int DayOfMonth,
    string DayOfWeekShort,
    string Status,
    string StatusCode,
    string InTime,
    string OutTime,
    decimal OvertimeHours);
