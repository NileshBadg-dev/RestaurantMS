namespace RestaurantMS.Application.Common.Models.Billing;

public record KotPrintDto(
    string KotNumber,
    DateTime CreatedAtUtc,
    string OrderType,
    string TableOrToken,
    string WaiterName,
    string Station,
    IReadOnlyList<KotPrintItemDto> Items,
    string? Notes = null,
    bool IsReprint = false);
