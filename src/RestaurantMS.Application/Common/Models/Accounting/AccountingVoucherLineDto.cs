namespace RestaurantMS.Application.Common.Models.Accounting;

public record AccountingVoucherLineDto(
    string AccountCode,
    string AccountName,
    decimal Debit,
    decimal Credit,
    string? LineNarration = null);
