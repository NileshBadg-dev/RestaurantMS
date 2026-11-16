using FluentAssertions;
using RestaurantMS.SharedKernel;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class ResultTests
{
    [Fact]
    public void Result_Success_HasSuccessStatus()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Result_Failure_HasErrorAndFailureStatus()
    {
        // Arrange
        var error = new Error("Test.Error", "Something failed");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ResultT_Success_ReturnsValue()
    {
        // Act
        var result = Result.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }
}
