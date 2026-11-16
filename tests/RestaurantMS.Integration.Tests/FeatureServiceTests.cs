using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Domain.Entities;
using RestaurantMS.Infrastructure.Services;
using Xunit;

namespace RestaurantMS.Integration.Tests;

public class FeatureServiceTests
{
    [Fact]
    public async Task IsEnabledAsync_FeatureEnabled_ReturnsTrue()
    {
        // Arrange
        var mockRepo = new Mock<IFeatureToggleRepository>();
        mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new FeatureToggle { Key = "Billing.Discounts", IsEnabled = true, Name = "Discounts" }
            ]);

        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new FeatureService(mockRepo.Object, memoryCache);

        // Act
        var isEnabled = await service.IsEnabledAsync("Billing.Discounts");

        // Assert
        isEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task IsEnabledAsync_FeatureDisabled_ReturnsFalse()
    {
        // Arrange
        var mockRepo = new Mock<IFeatureToggleRepository>();
        mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new FeatureToggle { Key = "Billing.Discounts", IsEnabled = false, Name = "Discounts" }
            ]);

        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new FeatureService(mockRepo.Object, memoryCache);

        // Act
        var isEnabled = await service.IsEnabledAsync("Billing.Discounts");

        // Assert
        isEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_CachesResults_SubsequentCallsDoNotHitRepository()
    {
        // Arrange
        var mockRepo = new Mock<IFeatureToggleRepository>();
        mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new FeatureToggle { Key = "Billing.Discounts", IsEnabled = true, Name = "Discounts" }
            ]);

        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new FeatureService(mockRepo.Object, memoryCache);

        // Act
        var first = await service.GetAllAsync();
        var second = await service.GetAllAsync();

        // Assert
        first.Should().BeEquivalentTo(second);
        mockRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateCache_ClearsCache_SubsequentCallHitsRepository()
    {
        // Arrange
        var mockRepo = new Mock<IFeatureToggleRepository>();
        mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new FeatureToggle { Key = "Billing.Discounts", IsEnabled = true, Name = "Discounts" }
            ]);

        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new FeatureService(mockRepo.Object, memoryCache);

        // Act
        await service.GetAllAsync();
        service.InvalidateCache();
        await service.GetAllAsync();

        // Assert
        mockRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
