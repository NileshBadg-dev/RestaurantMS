using RestaurantMS.Application.Common.Interfaces;

namespace RestaurantMS.Infrastructure.Services;

public class CurrentUserService : ICurrentUser
{
    public string? UserId => null;
    public string? UserName => "System";
    public IReadOnlyList<string> Roles => [];
    public IReadOnlyList<string> Permissions => [];
    public bool IsAuthenticated => false;
}
