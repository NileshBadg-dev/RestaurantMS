namespace RestaurantMS.Application.Roles.Common;

public class RolePermissionMatrixDto
{
    public long RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public IReadOnlyList<ScreenPermissionRowDto> Rows { get; init; } = [];
}
