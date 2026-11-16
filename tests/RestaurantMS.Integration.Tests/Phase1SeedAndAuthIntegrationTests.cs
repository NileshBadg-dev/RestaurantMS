using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantMS.Application.Authentication.Commands.Login;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Infrastructure.Persistence;
using RestaurantMS.Infrastructure.Persistence.Repositories;
using RestaurantMS.Infrastructure.Services;
using Xunit;

namespace RestaurantMS.Integration.Tests;

public class Phase1SeedAndAuthIntegrationTests
{
    private static AppDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_WhenRunTwice_ProducesNoDuplicatesAndIsIdempotent()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context1 = CreateInMemoryDbContext(dbName);
        var passwordHasher = new PasswordHasherService();

        // Act 1: Initial Seed
        await DbInitializer.SeedAsync(context1, passwordHasher);

        var permissionsCount1 = await context1.Permissions.CountAsync();
        var rolesCount1 = await context1.Roles.CountAsync();
        var rolePermsCount1 = await context1.RolePermissions.CountAsync();
        var settingsCount1 = await context1.HotelSettings.CountAsync();
        var togglesCount1 = await context1.FeatureToggles.CountAsync();
        var usersCount1 = await context1.Users.CountAsync();

        // Act 2: Second Seed on new context connected to same in-memory DB
        using var context2 = CreateInMemoryDbContext(dbName);
        await DbInitializer.SeedAsync(context2, passwordHasher);

        var permissionsCount2 = await context2.Permissions.CountAsync();
        var rolesCount2 = await context2.Roles.CountAsync();
        var rolePermsCount2 = await context2.RolePermissions.CountAsync();
        var settingsCount2 = await context2.HotelSettings.CountAsync();
        var togglesCount2 = await context2.FeatureToggles.CountAsync();
        var usersCount2 = await context2.Users.CountAsync();

        // Assert: Counts must match exactly
        rolesCount1.Should().Be(6); // Owner, Manager, Cashier, Waiter, Kitchen, Accountant
        rolesCount2.Should().Be(rolesCount1);
        permissionsCount1.Should().BeGreaterThan(0);
        permissionsCount2.Should().Be(permissionsCount1);
        rolePermsCount2.Should().Be(rolePermsCount1);
        settingsCount2.Should().Be(settingsCount1);
        togglesCount2.Should().Be(togglesCount1);
        usersCount1.Should().Be(1);
        usersCount2.Should().Be(1);
    }

    [Fact]
    public async Task SeededAdmin_CanLogin_SuccessfullyWithDefaultCredentials()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateInMemoryDbContext(dbName);
        var passwordHasher = new PasswordHasherService();
        await DbInitializer.SeedAsync(context, passwordHasher);

        var userRepo = new UserRepository(context);
        var roleRepo = new RoleRepository(context);

        var dateTimeMock = new Mock<IDateTime>();
        dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.UtcNow);

        var handler = new LoginCommandHandler(
            userRepo,
            roleRepo,
            passwordHasher,
            dateTimeMock.Object,
            context);

        var command = new LoginCommand("admin", "Admin@123", false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be("admin");
        result.Value.FullName.Should().Be("System Administrator");
        result.Value.RoleName.Should().Be("Owner");
        result.Value.MustChangePassword.Should().BeTrue();
        result.Value.Permissions.Should().Contain("Settings.View");
        result.Value.Permissions.Should().Contain("Users.Create");
        result.Value.Permissions.Should().Contain("Roles.Edit");
    }
}
