namespace RestaurantMS.Application.Roles.Common;

public class RoleDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsSystemRole { get; init; }
    public int UserCount { get; init; }
    public int PermissionCount { get; init; }
}
