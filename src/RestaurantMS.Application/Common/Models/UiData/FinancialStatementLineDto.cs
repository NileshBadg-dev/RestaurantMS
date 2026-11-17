namespace RestaurantMS.Application.Common.Models.UiData;

public record FinancialStatementLineDto(
    string AccountName,
    decimal Amount,
    bool IsHeader = false,
    bool IsTotal = false);
