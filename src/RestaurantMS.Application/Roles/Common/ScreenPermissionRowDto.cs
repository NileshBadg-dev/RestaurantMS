namespace RestaurantMS.Application.Roles.Common;

public class ScreenPermissionRowDto
{
    public string Screen { get; init; } = string.Empty;
    public PermissionCellDto? View { get; init; }
    public PermissionCellDto? Create { get; init; }
    public PermissionCellDto? Edit { get; init; }
    public PermissionCellDto? Delete { get; init; }
    public PermissionCellDto? Print { get; init; }
    public PermissionCellDto? Export { get; init; }
}
