using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class ChartOfAccountsViewModel
{
    public IReadOnlyList<AccountNodeDto> RootAccounts { get; set; } = Array.Empty<AccountNodeDto>();
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
}
