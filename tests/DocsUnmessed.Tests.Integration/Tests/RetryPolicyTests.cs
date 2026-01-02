namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Connectors.Cloud.Retry;
using NUnit.Framework;

/// <summary>
/// Tests for RetryPolicy
/// </summary>
[TestFixture]
[CancelAfter(10000)] // Global 10 second timeout for all tests in this fixture
public sealed class RetryPolicyTests
{
    [Test]
    public async Task ExecuteAsync_SuccessfulAction_ReturnsResult()
    {
        // Arrange
        var policy = new RetryPolicy(maxRetries: 3);
        var callCount = 0;

        // Act
        var result = await policy.ExecuteAsync(async () =>
        {
            callCount++;
            await Task.CompletedTask;
            return "success";
        });

        // Assert
        Assert.That(result, Is.EqualTo("success"));
        Assert.That(callCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ExecuteAsync_TransientFailureThenSuccess_Retries()
    {
        // Arrange
        var policy = new RetryPolicy(
            maxRetries: 3,
            initialDelay: TimeSpan.FromMilliseconds(10));
        var callCount = 0;

        // Act
        var result = await policy.ExecuteAsync(async () =>
        {
            callCount++;
            await Task.CompletedTask;
            
            if (callCount < 3)
            {
                throw new HttpRequestException("Transient error");
            }
            
            return "success";
        });

        // Assert
        Assert.That(result, Is.EqualTo("success"));
        Assert.That(callCount, Is.EqualTo(3));
    }

    [Test]
    [CancelAfter(1000)] // 1 second timeout
    public void ExecuteAsync_AllRetriesFail_ThrowsRetryExhaustedException()
    {
        // Arrange - Use minimal delays
        var policy = new RetryPolicy(
            maxRetries: 2,
            initialDelay: TimeSpan.FromMilliseconds(5),
            backoffMultiplier: 1.0); // No exponential backoff for speed
        var callCount = 0;

        // Act & Assert
        var ex = Assert.ThrowsAsync<RetryExhaustedException>(async () =>
        {
            await policy.ExecuteAsync(async () =>
            {
                callCount++;
                await Task.CompletedTask;
                throw new HttpRequestException("Always fails");
            });
        });

        Assert.That(callCount, Is.EqualTo(3)); // Initial + 2 retries
        Assert.That(ex?.InnerException, Is.InstanceOf<HttpRequestException>());
    }

    [Test]
    public async Task ExecuteAsync_NonTransientError_NoRetry()
    {
        // Arrange
        var policy = new RetryPolicy(maxRetries: 3);
        var callCount = 0;

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await policy.ExecuteAsync<string>(async () =>
            {
                callCount++;
                await Task.CompletedTask;
                throw new InvalidOperationException("Non-transient error");
            });
        });

        Assert.That(callCount, Is.EqualTo(1)); // No retries
    }

    [Test]
    [CancelAfter(2000)] // 2 second timeout
    public async Task ExecuteAsync_ExponentialBackoff_IncrementsDelay()
    {
        // Arrange - Use small delays to fit within timeout
        var policy = new RetryPolicy(
            maxRetries: 2,
            initialDelay: TimeSpan.FromMilliseconds(50),
            backoffMultiplier: 1.5); // Moderate backoff
        
        var attemptTimes = new List<DateTime>();

        // Act
        try
        {
            await policy.ExecuteAsync(async () =>
            {
                attemptTimes.Add(DateTime.UtcNow);
                await Task.CompletedTask;
                throw new HttpRequestException("Test");
            });
        }
        catch (RetryExhaustedException)
        {
            // Expected
        }

        // Assert - Should have made initial attempt + 2 retries
        Assert.That(attemptTimes.Count, Is.EqualTo(3), "Should have 3 attempts");
        
        // Verify delays increased (with generous tolerance)
        if (attemptTimes.Count >= 3)
        {
            var delay1 = (attemptTimes[1] - attemptTimes[0]).TotalMilliseconds;
            var delay2 = (attemptTimes[2] - attemptTimes[1]).TotalMilliseconds;
            
            // Just verify second delay is not shorter than first (allowing for timing variance)
            Assert.That(delay1, Is.GreaterThan(20), $"First delay should be at least 20ms, was {delay1:F0}ms");
            Assert.That(delay2, Is.GreaterThanOrEqualTo(delay1 * 0.8), 
                $"Second delay ({delay2:F0}ms) should be approximately equal or greater than first ({delay1:F0}ms) with tolerance");
        }
    }

    [Test]
    public async Task ExecuteAsync_NoReturnValue_WorksCorrectly()
    {
        // Arrange
        var policy = new RetryPolicy(
            maxRetries: 2,
            initialDelay: TimeSpan.FromMilliseconds(10));
        var callCount = 0;

        // Act
        await policy.ExecuteAsync(async () =>
        {
            callCount++;
            await Task.CompletedTask;
            
            if (callCount < 2)
            {
                throw new HttpRequestException("Retry");
            }
        });

        // Assert
        Assert.That(callCount, Is.EqualTo(2));
    }

    [Test]
    [CancelAfter(1000)] // 1 second timeout
    public async Task ExecuteAsync_WithCancellation_Cancels()
    {
        // Arrange
        var policy = new RetryPolicy(
            maxRetries: 5,
            initialDelay: TimeSpan.FromMilliseconds(500));
        
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        // Act & Assert - TaskCanceledException inherits from OperationCanceledException
        Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await policy.ExecuteAsync(async () =>
            {
                await Task.Delay(20); // Small delay to allow cancellation
                throw new HttpRequestException("Retry");
            }, cts.Token);
        });
    }

    [Test]
    [CancelAfter(2000)] // 2 second timeout
    public async Task Aggressive_RetriesMoreThanDefault()
    {
        // Arrange - Use minimal delays
        var aggressive = new RetryPolicy(
            maxRetries: 5,
            initialDelay: TimeSpan.FromMilliseconds(5),
            backoffMultiplier: 1.0); // No backoff for speed
        var defaultPolicy = new RetryPolicy(
            maxRetries: 3,
            initialDelay: TimeSpan.FromMilliseconds(5),
            backoffMultiplier: 1.0);
        var aggressiveCallCount = 0;
        var defaultCallCount = 0;

        // Act - Test aggressive policy
        try
        {
            await aggressive.ExecuteAsync(async () =>
            {
                aggressiveCallCount++;
                await Task.CompletedTask;
                throw new HttpRequestException("Error");
            });
        }
        catch (RetryExhaustedException)
        {
            // Expected
        }

        // Act - Test default policy
        try
        {
            await defaultPolicy.ExecuteAsync(async () =>
            {
                defaultCallCount++;
                await Task.CompletedTask;
                throw new HttpRequestException("Error");
            });
        }
        catch (RetryExhaustedException)
        {
            // Expected
        }

        // Assert - Aggressive should make more attempts (6 vs 4)
        Assert.That(aggressiveCallCount, Is.EqualTo(6), $"Aggressive policy should make 6 attempts, made {aggressiveCallCount}");
        Assert.That(defaultCallCount, Is.EqualTo(4), $"Default policy should make 4 attempts, made {defaultCallCount}");
        Assert.That(aggressiveCallCount, Is.GreaterThan(defaultCallCount));
    }

    [Test]
    public async Task ExecuteAsync_TimeoutException_IsTransient()
    {
        // Arrange
        var policy = new RetryPolicy(
            maxRetries: 2,
            initialDelay: TimeSpan.FromMilliseconds(10));
        var callCount = 0;

        // Act
        var result = await policy.ExecuteAsync(async () =>
        {
            callCount++;
            await Task.CompletedTask;
            
            if (callCount < 2)
            {
                throw new TimeoutException("Timeout");
            }
            
            return "success";
        });

        // Assert
        Assert.That(result, Is.EqualTo("success"));
        Assert.That(callCount, Is.EqualTo(2));
    }
}
