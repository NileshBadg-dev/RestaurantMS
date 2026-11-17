using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IBillingUiData
{
    Task<BillingOrderDetailsDto?> GetBillForTableAsync(string tableCode, CancellationToken cancellationToken = default);
    Task<PagedResult<BillSummaryRowDto>> GetBillsListAsync(DataTableRequest request, CancellationToken cancellationToken = default);
}
