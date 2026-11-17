using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class BillingPanelModel
{
    public BillingOrderDetailsDto Order { get; set; } = new("DineIn", "T2", 1260m, 5m, 63m, 1323m, "Ramesh Patil", "9822012345", 4250m, 10000m, new List<BillingItemRowDto>());
    public bool ShowCustomerPicker { get; set; } = true;
    public bool EnableCreditPayment { get; set; } = true;
    public string SettleActionUrl { get; set; } = "/Billing/Settle";
    public string HoldActionUrl { get; set; } = "/Billing/Hold";
    public string PrintActionUrl { get; set; } = "/Billing/Print";
}
