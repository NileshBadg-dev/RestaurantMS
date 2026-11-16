using RestaurantMS.Application.Common.Interfaces;

namespace RestaurantMS.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}
