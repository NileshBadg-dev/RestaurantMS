using FluentAssertions;
using Moq;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.Users.Commands.CreateUser;

namespace RestaurantMS.Application.Tests;

public class CreateUserCommandValidatorTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();

    public CreateUserCommandValidatorTests()
    {
        _userRepoMock.Setup(r => r.IsUserIdUniqueAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidUser_ShouldPass()
    {
        var validator = new CreateUserCommandValidator(_userRepoMock.Object);
        var command = new CreateUserCommand("john12", "John Doe", "john@test.com", "9876543210", "Password@1", 1);

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("abc")] // too short (<4)
    [InlineData("abcdefghijklmnopqrstu")] // too long (>20)
    [InlineData("user@name")] // invalid characters
    [InlineData("user name")] // space
    public async Task Validate_InvalidUserId_ShouldFail(string userId)
    {
        var validator = new CreateUserCommandValidator(_userRepoMock.Object);
        var command = new CreateUserCommand(userId, "John Doe", "john@test.com", "9876543210", "Password@1", 1);

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public async Task Validate_DuplicateUserId_ShouldFail()
    {
        _userRepoMock.Setup(r => r.IsUserIdUniqueAsync("admin", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var validator = new CreateUserCommandValidator(_userRepoMock.Object);
        var command = new CreateUserCommand("admin", "Admin User", "admin@test.com", "9876543210", "Password@1", 1);

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Theory]
    [InlineData("weak")] // too short
    [InlineData("password")] // no upper, no digit
    [InlineData("PASSWORD")] // no lower, no digit
    [InlineData("Password")] // no digit
    [InlineData("password12")] // no upper
    public async Task Validate_WeakPassword_ShouldFail(string password)
    {
        var validator = new CreateUserCommandValidator(_userRepoMock.Object);
        var command = new CreateUserCommand("john12", "John Doe", "john@test.com", "9876543210", password, 1);

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("1234567890")] // starts with 1
    [InlineData("5555555555")] // starts with 5
    [InlineData("98765")] // less than 10 digits
    [InlineData("98765432100")] // more than 10 digits
    public async Task Validate_InvalidMobile_ShouldFail(string mobile)
    {
        var validator = new CreateUserCommandValidator(_userRepoMock.Object);
        var command = new CreateUserCommand("john12", "John Doe", "john@test.com", mobile, "Password@1", 1);

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }
}
