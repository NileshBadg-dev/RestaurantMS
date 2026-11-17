namespace RestaurantMS.Web.Models;

public class ReservationFormViewModel
{
    public string Id { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
    public string ReservationTime { get; set; } = "19:30";
    public int GuestCount { get; set; } = 4;
    public string TableNumber { get; set; } = "T3";
    public decimal AdvanceAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
}
