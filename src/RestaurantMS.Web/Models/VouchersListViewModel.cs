using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class VouchersListViewModel
{
    public IReadOnlyList<VoucherItemDto> Vouchers { get; set; } = Array.Empty<VoucherItemDto>();
    public string SelectedType { get; set; } = "All"; // Payment, Receipt, Journal, Contra
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
}
