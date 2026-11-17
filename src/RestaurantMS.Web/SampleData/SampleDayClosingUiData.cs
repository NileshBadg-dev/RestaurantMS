using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleDayClosingUiData : IDayClosingUiData
{
    public Task<DayClosingSummaryDto> GetDayClosingSummaryAsync(DateTime? date = null, CancellationToken cancellationToken = default)
    {
        var summary = new DayClosingSummaryDto(
            DateFormatted: "16/11/2026",
            TotalBillsCount: 64,
            TotalSalesAmount: 48250m,
            CashCollected: 21450m,
            UpiCollected: 18600m,
            CardCollected: 5000m,
            UdhariCreditIssued: 3200m,
            CashPayouts: 1450m,
            ExpectedDrawerCash: 20000m,
            IsSettled: false);

        return Task.FromResult(summary);
    }

    public Task CloseDayAsync(decimal actualCash, string? remarks, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
