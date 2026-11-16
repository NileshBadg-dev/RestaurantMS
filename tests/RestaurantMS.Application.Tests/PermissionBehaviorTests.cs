using FluentAssertions;
using MediatR;
using Moq;
using RestaurantMS.Application.Common.Behaviors;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.SharedKernel;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class PermissionBehaviorTests
{
    private record TestSecuredCommand(string RequiredPermission) : IRequest<Result<string>>, IRequirePermission;
    private record TestUnsecuredCommand : IRequest<Result<string>>;

    [Fact]
    public async Task Handle_UserHasRequiredPermission_InvokesNextHandler()
    {
        // Arrange
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(u => u.Permissions).Returns(["Users.Create", "Users.View"]);

        var behavior = new PermissionBehavior<TestSecuredCommand, Result<string>>(currentUserMock.Object);
        var request = new TestSecuredCommand("Users.Create");

        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success("Success"));
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Success");
    }

    [Fact]
    public async Task Handle_UserLacksRequiredPermission_ReturnsFailureResultWithoutCallingNext()
    {
        // Arrange
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(u => u.Permissions).Returns(["Users.View"]);

        var behavior = new PermissionBehavior<TestSecuredCommand, Result<string>>(currentUserMock.Object);
        var request = new TestSecuredCommand("Users.Create");

        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success("Success"));
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.Forbidden");
    }

    [Fact]
    public async Task Handle_RequestDoesNotImplementIRequirePermission_InvokesNextHandler()
    {
        // Arrange
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(u => u.Permissions).Returns([]);

        var behavior = new PermissionBehavior<TestUnsecuredCommand, Result<string>>(currentUserMock.Object);
        var request = new TestUnsecuredCommand();

        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success("Ok"));
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }
}
