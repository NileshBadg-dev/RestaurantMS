using Microsoft.AspNetCore.Identity;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<AppUser> _hasher = new();
    private readonly AppUser _dummyUser = new();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(_dummyUser, password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(_dummyUser, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
