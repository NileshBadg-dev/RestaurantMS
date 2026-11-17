using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class CashBookViewModel
{
    public DateTime FromDate { get; set; } = DateTime.UtcNow.AddDays(-7);
    public DateTime ToDate { get; set; } = DateTime.UtcNow;
    public decimal OpeningBalance { get; set; }
    public decimal TotalReceipts { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal ClosingBalance { get; set; }
    public IReadOnlyList<DayBookEntryDto> Transactions { get; set; } = Array.Empty<DayBookEntryDto>();
}
