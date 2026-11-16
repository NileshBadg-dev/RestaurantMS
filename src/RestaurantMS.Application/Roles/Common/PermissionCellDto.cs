namespace RestaurantMS.Application.Roles.Common;

public class PermissionCellDto
{
    public long PermissionId { get; init; }
    public string Code { get; init; } = string.Empty;
    public bool IsGranted { get; init; }
}
