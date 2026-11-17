using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IStaffUiData
{
    Task<PagedResult<StaffMemberDto>> GetStaffListAsync(DataTableRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StaffMemberDto>> GetAllStaffAsync(CancellationToken cancellationToken = default);
    Task SaveStaffMemberAsync(string name, string role, string mobile, string shift, CancellationToken cancellationToken = default);
}
