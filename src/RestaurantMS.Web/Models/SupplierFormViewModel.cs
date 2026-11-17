namespace RestaurantMS.Web.Models;

public class SupplierFormViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Gstin { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int CreditDays { get; set; } = 30;
}
