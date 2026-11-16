using FluentAssertions;
using RestaurantMS.Domain.ValueObjects;
using Xunit;

namespace RestaurantMS.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Money_Addition_ReturnsSummedAmount()
    {
        // Arrange
        var m1 = new Money(100.50m, "INR");
        var m2 = new Money(50.25m, "INR");

        // Act
        var result = m1 + m2;

        // Assert
        result.Amount.Should().Be(150.75m);
        result.Currency.Should().Be("INR");
    }

    [Fact]
    public void Money_Subtraction_ReturnsDifference()
    {
        // Arrange
        var m1 = new Money(100.50m, "INR");
        var m2 = new Money(50.25m, "INR");

        // Act
        var result = m1 - m2;

        // Assert
        result.Amount.Should().Be(50.25m);
        result.Currency.Should().Be("INR");
    }

    [Fact]
    public void Money_DifferentCurrenciesAddition_ThrowsInvalidOperationException()
    {
        // Arrange
        var m1 = new Money(100m, "INR");
        var m2 = new Money(100m, "USD");

        // Act
        var act = () => _ = m1 + m2;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }
}
