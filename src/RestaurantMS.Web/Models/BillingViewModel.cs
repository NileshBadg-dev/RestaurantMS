using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class BillingViewModel
{
    public BillingOrderDetailsDto OrderDetails { get; set; } = new("DineIn", "T2", 1260m, 5.0m, 63m, 1323m, "Ramesh Patil", "9822012345", 4250m, 10000m, new List<BillingItemRowDto>());
    public IReadOnlyList<BillSummaryRowDto> TodayBills { get; set; } = new List<BillSummaryRowDto>();
    public decimal TodayTotalBilled { get; set; }
    public decimal TodayCashTotal { get; set; }
    public decimal TodayUpiTotal { get; set; }
    public decimal TodayCardTotal { get; set; }
    public decimal TodayCreditTotal { get; set; }
}
