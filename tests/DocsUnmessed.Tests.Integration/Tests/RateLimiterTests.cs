namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Connectors.Cloud.RateLimiting;
using NUnit.Framework;
using System.Diagnostics;

/// <summary>
/// Tests for RateLimiter
/// </summary>
[TestFixture]
[CancelAfter(10000)] // Global 10 second timeout for all tests in this fixture
public sealed class RateLimiterTests
{
    [Test]
    public async Task WaitAsync_BelowLimit_AllowsImmediately()
    {
        // Arrange
        using var limiter = new RateLimiter(maxRequests: 5, timeWindow: TimeSpan.FromSeconds(1));

        // Act
        var sw = Stopwatch.StartNew();
        await limiter.WaitAsync();
        await limiter.WaitAsync();
        await limiter.WaitAsync();
        sw.Stop();

        // Assert
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(100));
    }

    [Test]
    [CancelAfter(2000)] // 2 second timeout
    public async Task WaitAsync_AtLimit_Delays()
    {
        // Arrange
        using var limiter = new RateLimiter(maxRequests: 2, timeWindow: TimeSpan.FromMilliseconds(400));

        // Act - Fill the quota quickly
        var task1 = limiter.WaitAsync();
        var task2 = limiter.WaitAsync();
        await Task.WhenAll(task1, task2);

        // Act - This should wait for the time window
        var sw = Stopwatch.StartNew();
        await limiter.WaitAsync();
        sw.Stop();

        // Assert - Should have waited at least 250ms (allowing for timing variance)
        Assert.That(sw.ElapsedMilliseconds, Is.GreaterThan(250).And.LessThan(1000), 
            $"Expected delay between 250-1000ms, but was {sw.ElapsedMilliseconds}ms");
    }

    [Test]
    [CancelAfter(5000)] // 5 second timeout for this test
    public async Task WaitAsync_MultipleThreads_RespectsLimit()
    {
        // Arrange
        using var limiter = new RateLimiter(maxRequests: 10, timeWindow: TimeSpan.FromMilliseconds(500));
        var requestCount = 0;
        var completedTasks = 0;

        // Act - Start 20 concurrent requests
        var tasks = new List<Task>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await limiter.WaitAsync();
                Interlocked.Increment(ref requestCount);
                Interlocked.Increment(ref completedTasks);
            }));
        }

        // Wait a short time
        await Task.Delay(50);

        // Assert - Should not exceed the limit initially
        Assert.That(limiter.CurrentRequestCount, Is.LessThanOrEqualTo(10));

        // Wait for all to complete with timeout
        var completed = await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(4000));
        
        // Assert final count
        Assert.That(completedTasks, Is.GreaterThanOrEqualTo(10), "At least 10 tasks should complete");
    }

    [Test]
    public async Task CurrentRequestCount_ReflectsActiveRequests()
    {
        // Arrange
        using var limiter = new RateLimiter(maxRequests: 5, timeWindow: TimeSpan.FromSeconds(1));

        // Act & Assert
        Assert.That(limiter.CurrentRequestCount, Is.EqualTo(0));

        await limiter.WaitAsync();
        Assert.That(limiter.CurrentRequestCount, Is.EqualTo(1));

        await limiter.WaitAsync();
        Assert.That(limiter.CurrentRequestCount, Is.EqualTo(2));

        await limiter.WaitAsync();
        Assert.That(limiter.CurrentRequestCount, Is.EqualTo(3));
    }

    [Test]
    public async Task CurrentRequestCount_DecaysOverTime()
    {
        // Arrange
        using var limiter = new RateLimiter(maxRequests: 5, timeWindow: TimeSpan.FromMilliseconds(300));

        // Act
        await limiter.WaitAsync();
        await limiter.WaitAsync();
        Assert.That(limiter.CurrentRequestCount, Is.EqualTo(2));

        // Wait for time window to pass
        await Task.Delay(350);

        // Assert - Old requests should have expired
        Assert.That(limiter.CurrentRequestCount, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_InvalidMaxRequests_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new RateLimiter(maxRequests: 0, timeWindow: TimeSpan.FromSeconds(1)));

        Assert.Throws<ArgumentException>(() =>
            new RateLimiter(maxRequests: -1, timeWindow: TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void Constructor_InvalidTimeWindow_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new RateLimiter(maxRequests: 10, timeWindow: TimeSpan.Zero));

        Assert.Throws<ArgumentException>(() =>
            new RateLimiter(maxRequests: 10, timeWindow: TimeSpan.FromSeconds(-1)));
    }

    [Test]
    public async Task WaitAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequests: 5, timeWindow: TimeSpan.FromSeconds(1));
        limiter.Dispose();

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(async () => await limiter.WaitAsync());
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var limiter = new RateLimiter(maxRequests: 5, timeWindow: TimeSpan.FromSeconds(1));

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            limiter.Dispose();
            limiter.Dispose();
            limiter.Dispose();
        });
    }

    [Test]
    [CancelAfter(1000)] // 1 second timeout
    public async Task WaitAsync_WithCancellation_Cancels()
    {
        // Arrange
        using var limiter = new RateLimiter(maxRequests: 1, timeWindow: TimeSpan.FromSeconds(2));
        await limiter.WaitAsync(); // Fill quota

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50); // Cancel quickly

        // Act & Assert
        try
        {
            await limiter.WaitAsync(cts.Token);
            Assert.Fail("Expected OperationCanceledException");
        }
        catch (OperationCanceledException)
        {
            // Expected - test passes
            Assert.Pass("Cancellation worked as expected");
        }
    }

    [Test]
    public void RateLimitConfig_Default_HasValidValues()
    {
        // Act
        var config = RateLimitConfig.Default;

        // Assert
        Assert.That(config.MaxRequests, Is.GreaterThan(0));
        Assert.That(config.TimeWindow, Is.GreaterThan(TimeSpan.Zero));
    }

    [Test]
    public void RateLimitConfig_Conservative_IsSlowerThanDefault()
    {
        // Act
        var defaultConfig = RateLimitConfig.Default;
        var conservative = RateLimitConfig.Conservative;

        // Assert
        Assert.That(conservative.MaxRequests, Is.LessThanOrEqualTo(defaultConfig.MaxRequests));
    }

    [Test]
    public void RateLimitConfig_Aggressive_IsFasterThanDefault()
    {
        // Act
        var defaultConfig = RateLimitConfig.Default;
        var aggressive = RateLimitConfig.Aggressive;

        // Assert
        Assert.That(aggressive.MaxRequests, Is.GreaterThanOrEqualTo(defaultConfig.MaxRequests));
    }
}
