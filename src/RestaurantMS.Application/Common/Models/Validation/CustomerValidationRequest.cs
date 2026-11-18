namespace RestaurantMS.Application.Common.Models.Validation;

public record CustomerValidationRequest(
    string FullName,
    string Mobile,
    string? Email,
    string? Address,
    string? Gstin,
    string? Pan,
    decimal CreditLimit,
    bool CreditAllowed);
