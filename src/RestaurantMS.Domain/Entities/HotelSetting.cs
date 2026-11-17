using RestaurantMS.Domain.Enums;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Domain.Entities;

public class HotelSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public SettingDataType DataType { get; set; } = SettingDataType.String;
    public string Group { get; set; } = string.Empty;
    public string? Description { get; set; }
}
