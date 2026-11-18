namespace RestaurantMS.Application.Common.Models.Staff;

public record StaffProfileDto(
    long Id,
    string EmployeeCode,
    string Name,
    string Mobile,
    string Designation,
    string Shift,
    DateTime JoiningDate,
    string SalaryType,
    decimal BasicSalary,
    string IdProof,
    string BankOrUpi,
    string? PhotoUrl,
    bool IsActive,
    decimal MonthlyEquivalentSalary);
