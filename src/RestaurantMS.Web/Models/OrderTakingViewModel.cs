using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class OrderTakingViewModel
{
    public string TableId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string WaiterName { get; set; } = string.Empty;
    public int GuestCount { get; set; } = 2;
    public IReadOnlyList<TableItemDto> AvailableTables { get; set; } = new List<TableItemDto>();
    public IReadOnlyList<MenuCategoryDto> Categories { get; set; } = new List<MenuCategoryDto>();
    public IReadOnlyList<OrderItemMenuDto> AvailableMenuItems { get; set; } = new List<OrderItemMenuDto>();
    public string KitchenNotes { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
}
