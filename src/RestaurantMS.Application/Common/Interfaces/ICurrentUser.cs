namespace RestaurantMS.Application.Common.Interfaces;

public interface ICurrentUser
{
    string? UserId { get; }
    string? UserName { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
    bool IsAuthenticated { get; }
}
