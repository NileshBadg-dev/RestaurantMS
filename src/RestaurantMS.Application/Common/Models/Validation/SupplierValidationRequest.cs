namespace RestaurantMS.Application.Common.Models.Validation;

public record SupplierValidationRequest(
    string Name,
    string ContactPerson,
    string Mobile,
    string? Email,
    string? Gstin,
    string? Pan,
    string? UpiId);
