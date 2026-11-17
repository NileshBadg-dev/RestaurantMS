namespace RestaurantMS.Web.Models;

public class CustomerFormViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Gstin { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; } = 5000;
    public int CreditDays { get; set; } = 30;
    public bool IsCreditAllowed { get; set; } = true;
    public string Notes { get; set; } = string.Empty;
}
