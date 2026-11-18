using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Infrastructure.Persistence;
using RestaurantMS.Infrastructure.Persistence.Repositories;
using RestaurantMS.Infrastructure.Services;

namespace RestaurantMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=RestaurantMSDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
            {
                // Fallback to SqlServer if Pomelo not referenced, or Pomelo MySQL if present
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            }
            else
            {
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            }
        });

        services.AddMemoryCache();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IDateTime, DateTimeService>();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        // Phase 1 Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<IFeatureToggleRepository, FeatureToggleRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Phase 1 Services
        services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<ISettingService, SettingService>();
        services.AddScoped<IFeatureService, FeatureService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddSingleton<IAccountingService, InMemoryAccountingService>();

        return services;
    }
}
