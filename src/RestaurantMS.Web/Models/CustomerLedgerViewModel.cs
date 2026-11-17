using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class CustomerLedgerViewModel
{
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public IReadOnlyList<CustomerLedgerEntryDto> Entries { get; set; } = new List<CustomerLedgerEntryDto>();
}
