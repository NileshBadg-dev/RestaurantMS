using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface ICustomersUiData
{
    Task<PagedResult<CustomerListItemDto>> GetCustomersAsync(DataTableRequest request, CancellationToken cancellationToken = default);
    Task<CustomerListItemDto?> GetCustomerByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerLedgerEntryDto>> GetCustomerLedgerAsync(long customerId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerAgeingDto>> GetCustomerAgeingAsync(CancellationToken cancellationToken = default);
}
