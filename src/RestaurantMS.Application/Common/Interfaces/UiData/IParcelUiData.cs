using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IParcelUiData
{
    Task<PagedResult<ParcelItemDto>> GetParcelsAsync(DataTableRequest request, string? status = null, CancellationToken cancellationToken = default);
    Task<ParcelDetailsDto?> GetParcelDetailsAsync(string tokenNo, CancellationToken cancellationToken = default);
}
