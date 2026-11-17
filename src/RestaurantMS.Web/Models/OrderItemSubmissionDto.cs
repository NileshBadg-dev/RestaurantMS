namespace RestaurantMS.Web.Models;

public class OrderItemSubmissionDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal Rate { get; set; }
    public string? Notes { get; set; }
}
