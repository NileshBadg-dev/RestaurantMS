using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class SupplierListViewModel
{
    public IReadOnlyList<SupplierDto> Suppliers { get; set; } = Array.Empty<SupplierDto>();
    public int TotalSuppliers { get; set; }
    public decimal TotalPayable { get; set; }
}
