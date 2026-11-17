namespace RestaurantMS.Application.Common.Models.UiData;

public record VoucherItemDto(
    long Id,
    string VoucherNo,
    string DateFormatted,
    string Type,
    string Particulars,
    decimal DebitAmount,
    decimal CreditAmount);
