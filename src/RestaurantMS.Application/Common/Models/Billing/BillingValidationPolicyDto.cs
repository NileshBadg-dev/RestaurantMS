namespace RestaurantMS.Application.Common.Models.Billing;

public record BillingValidationPolicyDto(
    bool UpiReferenceRequired = false,
    bool CardDetailsRequired = true,
    string HotelUpiId = "grandrestaurant@okhdfc",
    string HotelName = "Grand Restaurant");
