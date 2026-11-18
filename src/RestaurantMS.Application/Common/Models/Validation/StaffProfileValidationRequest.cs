namespace RestaurantMS.Application.Common.Models.Validation;

public record StaffProfileValidationRequest(
    string EmployeeCode,
    string FullName,
    string Mobile,
    string? Email,
    DateTime JoiningDate,
    decimal BasicSalary,
    string? Pan,
    string? UpiId);
