namespace RestaurantMS.SharedKernel;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
