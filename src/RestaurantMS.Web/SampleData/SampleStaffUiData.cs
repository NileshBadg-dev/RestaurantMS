using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleStaffUiData : IStaffUiData
{
    private static readonly List<StaffMemberDto> StaffList =
    [
        new(1, "EMP-001", "Suresh Patil", "Head Waiter", "9822114455", "Dinner", true),
        new(2, "EMP-002", "Anil Deshmukh", "Waiter", "9822336677", "Dinner", true),
        new(3, "EMP-003", "Meena Jadhav", "Captain", "9822558899", "Dinner", true),
        new(4, "EMP-004", "Chef Rakesh", "Head Chef", "9822770011", "All Day", true),
        new(5, "EMP-005", "Prakash Shinde", "Cashier", "9822992233", "Evening", true),
        new(6, "EMP-006", "Kavita Pawar", "Kitchen Assistant", "9822004466", "Morning", false)
    ];

    public Task<PagedResult<StaffMemberDto>> GetStaffListAsync(DataTableRequest request, CancellationToken cancellationToken = default)
    {
        var all = StaffList;
        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim().ToLowerInvariant();
            all = all.Where(s => s.Name.ToLowerInvariant().Contains(search) || s.Role.ToLowerInvariant().Contains(search)).ToList();
        }

        var paged = all.Skip(request.Start).Take(request.PageSize).ToList();
        return Task.FromResult(new PagedResult<StaffMemberDto>(paged, all.Count, request.PageNumber, request.PageSize));
    }

    public Task<IReadOnlyList<StaffMemberDto>> GetAllStaffAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<StaffMemberDto>>(StaffList);
    }

    public Task SaveStaffMemberAsync(string name, string role, string mobile, string shift, CancellationToken cancellationToken = default)
    {
        StaffList.Add(new(StaffList.Count + 1, $"EMP-{StaffList.Count + 1:D3}", name, role, mobile, shift, true));
        return Task.CompletedTask;
    }
}
