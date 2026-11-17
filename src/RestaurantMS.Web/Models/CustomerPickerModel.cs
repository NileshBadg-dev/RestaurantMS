namespace RestaurantMS.Web.Models;

public class CustomerPickerModel
{
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerMobile { get; set; }
    public decimal? OutstandingAmount { get; set; }
    public decimal? CreditLimit { get; set; }
    public bool IsCreditAllowed { get; set; } = true;
    public string FieldName { get; set; } = "CustomerId";
}
