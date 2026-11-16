namespace RestaurantMS.Application.Authentication.Commands.Login;

public class LoginResultDto
{
    public bool Success { get; init; }
    public bool IsLockedOut { get; init; }
    public bool MustChangePassword { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public IReadOnlyList<string> Permissions { get; init; } = [];
}
