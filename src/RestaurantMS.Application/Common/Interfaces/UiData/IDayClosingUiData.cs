using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IDayClosingUiData
{
    Task<DayClosingSummaryDto> GetDayClosingSummaryAsync(DateTime? date = null, CancellationToken cancellationToken = default);
    Task CloseDayAsync(decimal actualCash, string? remarks, CancellationToken cancellationToken = default);
}
