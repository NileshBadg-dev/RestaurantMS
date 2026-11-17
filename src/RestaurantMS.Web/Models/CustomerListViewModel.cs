using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class CustomerListViewModel
{
    public IReadOnlyList<CustomerListItemDto> Customers { get; set; } = new List<CustomerListItemDto>();
    public decimal TotalOutstanding { get; set; }
    public int TotalCreditCustomers { get; set; }
    public decimal TotalCreditLimit { get; set; }
}
