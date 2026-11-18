namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterHotelInfoDto(
    string HotelNameEn,
    string HotelNameMr,
    string Gstin,
    string Fssai,
    string Address,
    string Phone,
    string Email,
    string Currency,
    string Timezone,
    string ReceiptHeader,
    string ReceiptFooter);
