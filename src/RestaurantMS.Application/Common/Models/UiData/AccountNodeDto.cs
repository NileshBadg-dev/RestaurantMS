namespace RestaurantMS.Application.Common.Models.UiData;

public record AccountNodeDto(
    string Code,
    string Name,
    string Type,
    decimal Balance,
    IReadOnlyList<AccountNodeDto> Children);
