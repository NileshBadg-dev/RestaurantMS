using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Infrastructure.Persistence;
using RestaurantMS.Infrastructure.Services;

namespace RestaurantMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=RestaurantMSDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IDateTime, DateTimeService>();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        return services;
    }
}
