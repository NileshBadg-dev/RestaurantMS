namespace RestaurantMS.Application.Common.Models.UiData;

public record StaffMemberDto(
    long Id,
    string EmployeeCode,
    string Name,
    string Role,
    string Mobile,
    string Shift,
    bool IsOnDuty);
