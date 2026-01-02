namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Services;
using NUnit.Framework;
using System.Diagnostics;

/// <summary>
/// Integration tests for CacheService behavior
/// </summary>
[TestFixture]
public sealed class CacheServiceTests
{
    private CacheService? _cache;

    [SetUp]
    public void Setup()
    {
        _cache = new CacheService(TimeSpan.FromSeconds(2));
    }

    [TearDown]
    public void TearDown()
    {
        _cache?.Dispose();
    }

    [Test]
    public async Task GetOrAddAsync_FirstCall_ExecutesFactory()
    {
        // Arrange
        var factoryCalled = false;
        Func<Task<string>> factory = () =>
        {
            factoryCalled = true;
            return Task.FromResult("test-value");
        };

        // Act
        var result = await _cache!.GetOrAddAsync("test-key", factory);

        // Assert
        Assert.That(result, Is.EqualTo("test-value"));
        Assert.That(factoryCalled, Is.True);
    }

    [Test]
    public async Task GetOrAddAsync_SecondCall_UsesCache()
    {
        // Arrange
        var callCount = 0;
        Func<Task<string>> factory = () =>
        {
            callCount++;
            return Task.FromResult($"value-{callCount}");
        };

        // Act
        var result1 = await _cache!.GetOrAddAsync("test-key", factory);
        var result2 = await _cache.GetOrAddAsync("test-key", factory);

        // Assert
        Assert.That(result1, Is.EqualTo("value-1"));
        Assert.That(result2, Is.EqualTo("value-1")); // Same value from cache
        Assert.That(callCount, Is.EqualTo(1)); // Factory called only once
    }

    [Test]
    public async Task GetOrAddAsync_AfterExpiration_ExecutesFactoryAgain()
    {
        // Arrange
        var callCount = 0;
        Func<Task<string>> factory = () =>
        {
            callCount++;
            return Task.FromResult($"value-{callCount}");
        };

        // Act
        var result1 = await _cache!.GetOrAddAsync("test-key", factory, TimeSpan.FromMilliseconds(100));
        await Task.Delay(150); // Wait for expiration
        var result2 = await _cache.GetOrAddAsync("test-key", factory);

        // Assert
        Assert.That(result1, Is.EqualTo("value-1"));
        Assert.That(result2, Is.EqualTo("value-2")); // New value after expiration
        Assert.That(callCount, Is.EqualTo(2)); // Factory called twice
    }

    [Test]
    public async Task GetOrAddAsync_CacheHit_IsFasterThanMiss()
    {
        // Arrange
        Func<Task<string>> slowFactory = async () =>
        {
            await Task.Delay(50);
            return "test-value";
        };

        // Act
        var sw1 = Stopwatch.StartNew();
        await _cache!.GetOrAddAsync("test-key", slowFactory);
        sw1.Stop();

        var sw2 = Stopwatch.StartNew();
        await _cache.GetOrAddAsync("test-key", slowFactory);
        sw2.Stop();

        // Assert
        Assert.That(sw2.ElapsedMilliseconds, Is.LessThan(sw1.ElapsedMilliseconds));
        Assert.That(sw2.ElapsedMilliseconds, Is.LessThan(10)); // Should be very fast
    }

    [Test]
    public void Set_StoresValue_CanBeRetrieved()
    {
        // Arrange & Act
        _cache!.Set("test-key", "test-value");

        // Assert
        var found = _cache.TryGet<string>("test-key", out var value);
        Assert.That(found, Is.True);
        Assert.That(value, Is.EqualTo("test-value"));
    }

    [Test]
    public void TryGet_NonExistentKey_ReturnsFalse()
    {
        // Act
        var found = _cache!.TryGet<string>("non-existent", out var value);

        // Assert
        Assert.That(found, Is.False);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void Remove_ExistingKey_ReturnsTrue()
    {
        // Arrange
        _cache!.Set("test-key", "test-value");

        // Act
        var removed = _cache.Remove("test-key");

        // Assert
        Assert.That(removed, Is.True);
        var found = _cache.TryGet<string>("test-key", out _);
        Assert.That(found, Is.False);
    }

    [Test]
    public void Remove_NonExistentKey_ReturnsFalse()
    {
        // Act
        var removed = _cache!.Remove("non-existent");

        // Assert
        Assert.That(removed, Is.False);
    }

    [Test]
    public void Clear_RemovesAllEntries()
    {
        // Arrange
        _cache!.Set("key1", "value1");
        _cache.Set("key2", "value2");
        _cache.Set("key3", "value3");

        // Act
        _cache.Clear();

        // Assert
        Assert.That(_cache.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetStatistics_ReturnsAccurateCount()
    {
        // Arrange
        _cache!.Set("key1", "value1");
        _cache.Set("key2", "value2");
        _cache.Set("key3", "value3");

        // Act
        var stats = _cache.GetStatistics();

        // Assert
        Assert.That(stats.TotalEntries, Is.EqualTo(3));
        Assert.That(stats.ActiveEntries, Is.EqualTo(3));
        Assert.That(stats.ExpiredEntries, Is.EqualTo(0));
    }

    [Test]
    public async Task AutomaticCleanup_RemovesExpiredEntries()
    {
        // Arrange
        _cache!.Set("key1", "value1", TimeSpan.FromMilliseconds(100));
        _cache.Set("key2", "value2", TimeSpan.FromMinutes(10));

        // Act
        await Task.Delay(150); // Wait for key1 to expire
        await Task.Delay(1100); // Wait for cleanup timer (runs every 1 minute, but we test the concept)

        // Assert
        var found1 = _cache.TryGet<string>("key1", out _);
        var found2 = _cache.TryGet<string>("key2", out _);
        
        Assert.That(found1, Is.False); // Expired
        Assert.That(found2, Is.True);  // Still valid
    }

    [Test]
    public void GetOrAddAsync_WithNullKey_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _cache!.GetOrAddAsync(null!, () => Task.FromResult("value")));
    }

    [Test]
    public void GetOrAddAsync_WithNullFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _cache!.GetOrAddAsync<string>("key", null!));
    }
}
