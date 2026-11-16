using FluentAssertions;
using Moq;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.Roles.Commands.UpdateRole;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class UpdateRoleCommandValidatorTests
{
    private readonly Mock<IRoleRepository> _roleRepoMock = new();

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        _roleRepoMock.Setup(r => r.IsNameUniqueAsync("Supervisor", 1L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var validator = new UpdateRoleCommandValidator(_roleRepoMock.Object);
        var command = new UpdateRoleCommand(1L, "Supervisor", "Shift supervisor");

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_DuplicateRoleName_FailsValidation()
    {
        _roleRepoMock.Setup(r => r.IsNameUniqueAsync("ExistingRole", 1L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var validator = new UpdateRoleCommandValidator(_roleRepoMock.Object);
        var command = new UpdateRoleCommand(1L, "ExistingRole", "Duplicate");

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateRoleCommand.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_InvalidId_FailsValidation(long id)
    {
        var validator = new UpdateRoleCommandValidator(_roleRepoMock.Object);
        var command = new UpdateRoleCommand(id, "ValidRole", null);

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateRoleCommand.Id));
    }
}
