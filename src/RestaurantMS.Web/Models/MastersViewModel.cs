namespace RestaurantMS.Web.Models;

public class MastersViewModel
{
    public IReadOnlyList<MasterItemModel> Masters { get; init; } = Array.Empty<MasterItemModel>();
}
