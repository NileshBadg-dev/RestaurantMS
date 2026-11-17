namespace RestaurantMS.Application.Common.Models.UiData;

public record CustomerLedgerEntryDto(
    string DateFormatted,
    string Description,
    string ReferenceNumber,
    decimal DebitAmount,
    decimal CreditAmount,
    decimal RunningBalance);
