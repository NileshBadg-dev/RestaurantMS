namespace RestaurantMS.Application.Common.Models.Accounting;

public record PostVoucherRequest(
    DateTime VoucherDate,
    string VoucherType,
    string Narration,
    string? ReferenceNumber,
    IReadOnlyList<AccountingVoucherLineDto> Lines);
