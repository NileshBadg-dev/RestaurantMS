using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Web.Models;

public class PayslipViewModel
{
    public string HotelName { get; init; } = "Grand Restaurant";
    public string HotelAddress { get; init; } = string.Empty;
    public StaffProfileDto Profile { get; init; } = null!;
    public SalaryStatementRowDto Salary { get; init; } = null!;
    public string MonthYearLabel { get; init; } = string.Empty;
}
