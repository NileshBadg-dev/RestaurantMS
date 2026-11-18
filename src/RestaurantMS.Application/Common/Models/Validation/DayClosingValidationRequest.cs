namespace RestaurantMS.Application.Common.Models.Validation;

public record DayClosingValidationRequest(
    DateTime ClosingDate,
    decimal OpeningFloat,
    decimal ExpectedCash,
    decimal CountedCash,
    string? VarianceRemarks);
