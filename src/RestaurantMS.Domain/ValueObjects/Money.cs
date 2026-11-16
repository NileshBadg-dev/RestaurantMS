namespace RestaurantMS.Domain.ValueObjects;

public record Money(decimal Amount, string Currency = "INR")
{
    public static Money Zero(string currency = "INR") => new(0m, currency);

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {a.Currency} and {b.Currency}");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {a.Currency} and {b.Currency}");
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal multiplier) => new(a.Amount * multiplier, a.Currency);
    public static Money operator /(Money a, decimal divisor) => new(a.Amount / divisor, a.Currency);
}
