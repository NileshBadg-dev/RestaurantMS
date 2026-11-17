namespace RestaurantMS.Application.Common.Models.UiData;

public record DayBookEntryDto(
    string Time,
    string VoucherNo,
    string AccountName,
    string VoucherType,
    decimal DebitAmount,
    decimal CreditAmount,
    string? Narration);
