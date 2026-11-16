using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantMS.Application;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Infrastructure;
using Xunit;

namespace RestaurantMS.Integration.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void ServiceProvider_ResolvesCoreDependencies_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=RestaurantMSTestDb;Trusted_Connection=True;"
            })
            .Build();

        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(configuration);

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<IDateTime>().Should().NotBeNull();
        provider.GetRequiredService<ICurrentUser>().Should().NotBeNull();
        provider.GetRequiredService<IUnitOfWork>().Should().NotBeNull();
    }
}
