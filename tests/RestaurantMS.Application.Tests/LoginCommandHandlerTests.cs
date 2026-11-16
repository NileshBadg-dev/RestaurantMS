using FluentAssertions;
using Moq;
using RestaurantMS.Application.Authentication.Commands.Login;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Domain.Entities;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<IPasswordHasherService> _passwordHasherMock = new();
    private readonly Mock<IDateTime> _dateTimeMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DateTime _utcNow = new(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc);

    public LoginCommandHandlerTests()
    {
        _dateTimeMock.Setup(d => d.UtcNow).Returns(_utcNow);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessAndResetsFailedCount()
    {
        // Arrange
        var user = new AppUser
        {
            Id = 1L,
            UserId = "admin",
            FullName = "Administrator",
            PasswordHash = "hashed_pass",
            IsActive = true,
            AccessFailedCount = 2,
            RoleId = 10L,
            MustChangePassword = true
        };

        var role = new Role
        {
            Id = 10L,
            Name = "Owner"
        };
        var perm = new Permission { Id = 100L, Code = "Settings.View", Screen = "Settings", Action = "View", Description = "View Settings" };
        role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = perm.Id, Permission = perm });

        _userRepoMock.Setup(r => r.GetByUserIdAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyPassword("hashed_pass", "Admin@123"))
            .Returns(true);
        _roleRepoMock.Setup(r => r.GetByIdAsync(user.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _roleRepoMock.Setup(r => r.GetWithPermissionsAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var handler = new LoginCommandHandler(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object,
            _unitOfWorkMock.Object);

        var command = new LoginCommand("admin", "Admin@123", false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be("admin");
        result.Value.MustChangePassword.Should().BeTrue();
        result.Value.Permissions.Should().Contain("Settings.View");
        user.AccessFailedCount.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongPassword_IncrementsFailedCountAndReturnsGenericError()
    {
        // Arrange
        var user = new AppUser
        {
            Id = 1L,
            UserId = "admin",
            FullName = "Administrator",
            PasswordHash = "hashed_pass",
            IsActive = true,
            AccessFailedCount = 2,
            RoleId = 10L
        };

        _userRepoMock.Setup(r => r.GetByUserIdAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyPassword("hashed_pass", "WrongPassword"))
            .Returns(false);

        var handler = new LoginCommandHandler(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object,
            _unitOfWorkMock.Object);

        var command = new LoginCommand("admin", "WrongPassword", false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        user.AccessFailedCount.Should().Be(3);
        user.LockoutEnd.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FifthFailedAttempt_LocksOutUserFor10Minutes()
    {
        // Arrange
        var user = new AppUser
        {
            Id = 2L,
            UserId = "cashier1",
            FullName = "Cashier User",
            PasswordHash = "hashed_pass",
            IsActive = true,
            AccessFailedCount = 4,
            RoleId = 20L
        };

        _userRepoMock.Setup(r => r.GetByUserIdAsync("cashier1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyPassword("hashed_pass", "WrongPassword"))
            .Returns(false);

        var handler = new LoginCommandHandler(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object,
            _unitOfWorkMock.Object);

        var command = new LoginCommand("cashier1", "WrongPassword", false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.LockedOut");
        user.AccessFailedCount.Should().Be(5);
        user.LockoutEnd.Should().Be(_utcNow.AddMinutes(10));
    }

    [Fact]
    public async Task Handle_CurrentlyLockedOutUser_ReturnsLockoutErrorWithoutCheckingPassword()
    {
        // Arrange
        var user = new AppUser
        {
            Id = 3L,
            UserId = "lockeduser",
            FullName = "Locked User",
            PasswordHash = "hashed_pass",
            IsActive = true,
            AccessFailedCount = 5,
            LockoutEnd = _utcNow.AddMinutes(7),
            RoleId = 20L
        };

        _userRepoMock.Setup(r => r.GetByUserIdAsync("lockeduser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new LoginCommandHandler(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object,
            _unitOfWorkMock.Object);

        var command = new LoginCommand("lockeduser", "Admin@123", false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.LockedOut");
        _passwordHasherMock.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsInactiveError()
    {
        // Arrange
        var user = new AppUser
        {
            Id = 4L,
            UserId = "inactiveuser",
            FullName = "Inactive User",
            PasswordHash = "hashed_pass",
            IsActive = false,
            RoleId = 20L
        };

        _userRepoMock.Setup(r => r.GetByUserIdAsync("inactiveuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new LoginCommandHandler(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object,
            _unitOfWorkMock.Object);

        var command = new LoginCommand("inactiveuser", "Admin@123", false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.InactiveUser");
    }

    [Fact]
    public async Task Handle_NonExistentUser_ReturnsGenericInvalidCredentialsError()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByUserIdAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);

        var handler = new LoginCommandHandler(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object,
            _unitOfWorkMock.Object);

        var command = new LoginCommand("unknown", "Admin@123", false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }
}
