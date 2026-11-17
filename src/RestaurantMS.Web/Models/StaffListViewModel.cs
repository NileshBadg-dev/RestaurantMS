using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class StaffListViewModel
{
    public IReadOnlyList<StaffMemberDto> StaffMembers { get; set; } = Array.Empty<StaffMemberDto>();
    public int TotalStaff { get; set; }
    public int PresentToday { get; set; }
    public int OnLeaveToday { get; set; }
}
