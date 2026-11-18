namespace RestaurantMS.Application.Common.Models.Accounting;

public record PostedVoucherDto(
    long VoucherId,
    string VoucherNumber,
    DateTime VoucherDate,
    string VoucherType,
    string Narration,
    string? ReferenceNumber,
    decimal TotalDebit,
    decimal TotalCredit,
    IReadOnlyList<AccountingVoucherLineDto> Lines);
