using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RestaurantMS.Application.Billing.Services;
using RestaurantMS.Application.Common.Behaviors;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Validation;
using RestaurantMS.Application.Inventory.Services;
using RestaurantMS.Application.Reports.Services;
using RestaurantMS.Application.Staff.Services;

namespace RestaurantMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);
        services.AddSingleton<ISalaryCalculationService, SalaryCalculationService>();
        services.AddSingleton<IBillingService, BillingService>();
        services.AddSingleton<IReceiptRenderer, ReceiptRenderer>();
        services.AddSingleton<InventoryStateStore>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddSingleton<ReportsStateStore>();
        services.AddSingleton<ReportDataGenerator>();
        services.AddSingleton<IReportsService, ReportsService>();
        services.AddSingleton<IOperationalValidationService, OperationalValidationService>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PermissionBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
        });

        return services;
    }
}
