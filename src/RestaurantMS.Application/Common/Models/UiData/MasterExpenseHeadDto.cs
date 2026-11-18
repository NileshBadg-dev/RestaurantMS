namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterExpenseHeadDto(
    long Id,
    string Name,
    string Category,
    decimal MonthlyBudget,
    bool IsTaxDeductible,
    string Status);
