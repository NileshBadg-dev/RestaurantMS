namespace RestaurantMS.Application.Common.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string userId, string permissionCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
}
