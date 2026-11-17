using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IReportsUiData
{
    Task<IReadOnlyList<ReportHubCardDto>> GetReportHubCardsAsync(CancellationToken cancellationToken = default);
    Task<ReportResultDto> GetReportDataAsync(string reportKey, DateTime? from = null, DateTime? to = null, string? filter = null, CancellationToken cancellationToken = default);
}
