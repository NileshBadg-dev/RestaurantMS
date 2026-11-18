namespace RestaurantMS.Web.Models;

public class QuickAddCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; } = 5000m;
    public bool CreditAllowed { get; set; } = true;
}
