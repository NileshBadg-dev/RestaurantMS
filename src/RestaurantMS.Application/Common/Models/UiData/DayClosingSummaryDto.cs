namespace RestaurantMS.Application.Common.Models.UiData;

public record DayClosingSummaryDto(
    string DateFormatted,
    int TotalBillsCount,
    decimal TotalSalesAmount,
    decimal CashCollected,
    decimal UpiCollected,
    decimal CardCollected,
    decimal UdhariCreditIssued,
    decimal CashPayouts,
    decimal ExpectedDrawerCash,
    bool IsSettled);
