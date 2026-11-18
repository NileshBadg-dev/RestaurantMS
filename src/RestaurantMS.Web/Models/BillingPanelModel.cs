using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class BillingPanelModel
{
    public BillingOrderDetailsDto Order { get; set; } = new("Dine-in", "T1", 0m, 5m, 0m, 0m, "Walk-in Customer", "", 0m, 10000m, []);
    public bool ShowCustomerPicker { get; set; } = true;
    public bool EnableCreditPayment { get; set; } = true;
    public string SettleActionUrl { get; set; } = "/Billing/Settle";
    public string CalculateActionUrl { get; set; } = "/Billing/Calculate";
    public string HoldActionUrl { get; set; } = "/Billing/Hold";
    public string PrintActionUrl { get; set; } = "/Receipt/PreviewDraft";
    public string HotelName { get; set; } = "Grand Restaurant";
    public string HotelUpiId { get; set; } = "grandrestaurant@okhdfc";
    public bool UpiReferenceRequired { get; set; } = false;
    public bool CardDetailsRequired { get; set; } = true;
    public bool AutoPrintOnSettle { get; set; } = true;
    public string PaperSize { get; set; } = "80mm";
    public bool CanOverrideCreditLimit { get; set; } = true;
    public IReadOnlyList<MenuItemDetailsDto> MenuCatalog { get; set; } = [];
    public IReadOnlyList<CustomerListItemDto> Customers { get; set; } = [];
}
