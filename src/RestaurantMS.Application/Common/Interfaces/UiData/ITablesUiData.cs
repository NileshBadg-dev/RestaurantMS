using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface ITablesUiData
{
    Task<IReadOnlyList<TableItemDto>> GetTablesAsync(string? section = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TableSectionDto>> GetSectionsAsync(CancellationToken cancellationToken = default);
    Task<TableDetailsDto?> GetTableDetailsAsync(string tableCode, CancellationToken cancellationToken = default);
}
