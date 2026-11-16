using FluentAssertions;
using Moq;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.Roles.Commands.CreateRole;

namespace RestaurantMS.Application.Tests;

public class CreateRoleCommandValidatorTests
{
    private readonly Mock<IRoleRepository> _roleRepoMock = new();

    public CreateRoleCommandValidatorTests()
    {
        _roleRepoMock.Setup(r => r.IsNameUniqueAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidRole_ShouldPass()
    {
        var validator = new CreateRoleCommandValidator(_roleRepoMock.Object);
        var command = new CreateRoleCommand("Captain", "Captain of waiters");

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyRoleName_ShouldFail()
    {
        var validator = new CreateRoleCommandValidator(_roleRepoMock.Object);
        var command = new CreateRoleCommand("", "Description");

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_DuplicateRoleName_ShouldFail()
    {
        _roleRepoMock.Setup(r => r.IsNameUniqueAsync("Owner", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var validator = new CreateRoleCommandValidator(_roleRepoMock.Object);
        var command = new CreateRoleCommand("Owner", "Owner role");

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }
}
