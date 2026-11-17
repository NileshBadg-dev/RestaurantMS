using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class CreateParcelViewModel
{
    public string CustomerName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string OrderType { get; set; } = "Takeaway"; // Takeaway, Delivery
    public string PaymentMode { get; set; } = "Cash"; // Cash, UPI, Card, Udhari
    public string Notes { get; set; } = string.Empty;
    public List<BillingItemRowDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DeliveryCharges { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
}
