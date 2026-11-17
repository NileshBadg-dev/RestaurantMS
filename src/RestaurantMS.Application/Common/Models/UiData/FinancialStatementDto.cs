namespace RestaurantMS.Application.Common.Models.UiData;

public record FinancialStatementDto(
    string Title,
    string PeriodOrAsOf,
    IReadOnlyList<FinancialStatementLineDto> LeftLines,
    IReadOnlyList<FinancialStatementLineDto> RightLines,
    decimal TotalLeft,
    decimal TotalRight);
