namespace RestaurantMS.Application.Common.Models.Staff;

public record AttendanceEntryDto(
    long StaffId,
    string EmployeeCode,
    string EmployeeName,
    string Designation,
    string Shift,
    DateTime Date,
    string Status,
    string InTime,
    string OutTime,
    decimal OvertimeHours,
    string Remarks);
