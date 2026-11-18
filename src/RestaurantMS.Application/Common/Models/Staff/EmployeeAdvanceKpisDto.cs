namespace RestaurantMS.Application.Common.Models.Staff;

public record EmployeeAdvanceKpisDto(
    decimal TotalAdvanceGiven,
    decimal RecoveredThisMonth,
    decimal TotalOutstanding,
    int StaffWithOutstandingCount);
