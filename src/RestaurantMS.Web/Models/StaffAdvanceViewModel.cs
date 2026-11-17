namespace RestaurantMS.Web.Models;

public class StaffAdvanceViewModel
{
    public IReadOnlyList<StaffAdvanceRowModel> Advances { get; set; } = Array.Empty<StaffAdvanceRowModel>();
    public decimal TotalAdvancesGiven => Advances.Sum(a => a.AdvanceAmount);
    public decimal TotalRecovered => Advances.Sum(a => a.RecoveredAmount);
    public decimal TotalOutstandingAdvance => Advances.Sum(a => a.BalanceAmount);
    public int ActiveAdvanceAccountsCount => Advances.Count(a => a.BalanceAmount > 0);
}
