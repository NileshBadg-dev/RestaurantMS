using RestaurantMS.Application.Common.Interfaces.UiData;

namespace RestaurantMS.Web.SampleData;

public static class SampleUiDataServiceExtensions
{
    public static IServiceCollection AddSampleUiData(this IServiceCollection services, bool useSampleData = true)
    {
        if (useSampleData)
        {
            services.AddScoped<IDashboardUiData, SampleDashboardUiData>();
            services.AddScoped<ITablesUiData, SampleTablesUiData>();
            services.AddScoped<IParcelUiData, SampleParcelUiData>();
            services.AddScoped<IOrdersUiData, SampleOrdersUiData>();
            services.AddScoped<IKitchenUiData, SampleKitchenUiData>();
            services.AddScoped<IBillingUiData, SampleBillingUiData>();
            services.AddScoped<ICustomersUiData, SampleCustomersUiData>();
            services.AddScoped<IMenuUiData, SampleMenuUiData>();
            services.AddScoped<IInventoryUiData, SampleInventoryUiData>();
            services.AddScoped<IAccountsUiData, SampleAccountsUiData>();
            services.AddScoped<IReportsUiData, SampleReportsUiData>();
            services.AddScoped<IReservationsUiData, SampleReservationsUiData>();
            services.AddSingleton<SampleStaffStore>();
            services.AddSingleton<SampleAdvanceStateStore>();
            services.AddSingleton<SampleAttendanceSalaryStore>();
            services.AddScoped<IEmployeeAdvancesUiData, SampleEmployeeAdvancesUiData>();
            services.AddScoped<IStaffUiData, SampleStaffUiData>();
            services.AddScoped<IDayClosingUiData, SampleDayClosingUiData>();
            services.AddScoped<IOnlineOrdersUiData, SampleOnlineOrdersUiData>();
            services.AddScoped<IAuditUiData, SampleAuditUiData>();
            services.AddSingleton<IMastersUiData, SampleMastersUiData>();
        }

        return services;
    }
}
