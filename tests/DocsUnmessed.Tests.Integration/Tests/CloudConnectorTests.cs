namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Connectors.Cloud;
using DocsUnmessed.Connectors.Cloud.RateLimiting;
using DocsUnmessed.Connectors.Cloud.Retry;
using DocsUnmessed.Core.Domain;
using DocsUnmessed.Tests.Integration.Mocks;
using NUnit.Framework;

/// <summary>
/// Integration tests for Cloud Connectors
/// </summary>
[TestFixture]
[CancelAfter(15000)] // Global 15 second timeout for cloud connector tests
public sealed class CloudConnectorTests
{
    [Test]
    public async Task AuthenticateAsync_ValidCredentials_Succeeds()
    {
        // Arrange
        using var connector = new MockCloudConnector();
        var credentials = new CloudCredentials
        {
            Type = AuthenticationType.OAuth2,
            AccessToken = "test-token"
        };

        // Act
        var result = await connector.AuthenticateAsync(credentials);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(connector.IsAuthenticated, Is.True);
        Assert.That(result.UserInfo, Is.Not.Null);
        Assert.That(result.UserInfo!.UserId, Is.Not.Null);
    }

    [Test]
    public async Task ListItemsAsync_ReturnsItems()
    {
        // Arrange
        using var connector = new MockCloudConnector();
        
        var item = new Item
        {
            Path = "/test/file.txt",
            Name = "file.txt",
            Provider = "Mock",
            Size = 1024,
            Type = ItemType.File,
            MimeType = "text/plain",
            CreatedUtc = DateTime.UtcNow,
            ModifiedUtc = DateTime.UtcNow,
            ExtendedProperties = new Dictionary<string, string>(),
            Issues = new List<string>(),
            IsShared = false,
            Depth = 1
        };
        
        connector.AddMockItem(item);

        // Act
        var items = await connector.ListItemsAsync("/test");

        // Assert
        Assert.That(items.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task UploadFileAsync_CreatesItem()
    {
        // Arrange
        using var connector = new MockCloudConnector();

        // Act
        await connector.UploadFileAsync("local.txt", "/remote/file.txt");

        // Assert
        var item = await connector.GetItemAsync("/remote/file.txt");
        Assert.That(item, Is.Not.Null);
    }

    [Test]
    public async Task DownloadFileAsync_ReportsProgress()
    {
        // Arrange
        using var connector = new MockCloudConnector();
        
        var item = new Item
        {
            Path = "/test/file.txt",
            Name = "file.txt",
            Provider = "Mock",
            Size = 1024,
            Type = ItemType.File,
            MimeType = "text/plain",
            CreatedUtc = DateTime.UtcNow,
            ModifiedUtc = DateTime.UtcNow,
            ExtendedProperties = new Dictionary<string, string>(),
            Issues = new List<string>(),
            IsShared = false,
            Depth = 1
        };
        
        connector.AddMockItem(item);

        var progressReports = new List<TransferProgress>();
        var progress = new Progress<TransferProgress>(p => progressReports.Add(p));

        // Act
        await connector.DownloadFileAsync("/test/file.txt", "local.txt", progress);

        // Assert
        Assert.That(progressReports.Count, Is.GreaterThan(0));
        Assert.That(progressReports.Last().PercentComplete, Is.EqualTo(100).Within(0.1));
    }

    [Test]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        using var connector = new MockCloudConnector();
        await connector.UploadFileAsync("local.txt", "/remote/file.txt");

        // Act
        await connector.DeleteAsync("/remote/file.txt");

        // Assert
        var item = await connector.GetItemAsync("/remote/file.txt");
        Assert.That(item, Is.Null);
    }

    [Test]
    [CancelAfter(2000)] // 2 second timeout
    public async Task Connector_WithRateLimiting_ThrottlesRequests()
    {
        // Arrange
        var config = new RateLimitConfig
        {
            MaxRequests = 10,
            TimeWindow = TimeSpan.FromMilliseconds(500)
        };
        
        using var connector = new MockCloudConnector(rateLimitConfig: config);

        // Act - Make many requests
        var tasks = new List<Task>();
        for (int i = 0; i < 15; i++)
        {
            tasks.Add(connector.ListItemsAsync("/test"));
        }

        await Task.WhenAll(tasks);

        // Assert - Should have completed without throwing
        Assert.That(connector.ApiCallCount, Is.EqualTo(15));
    }

    [Test]
    [CancelAfter(1000)] // 1 second timeout
    public async Task Connector_WithRetryPolicy_RetriesOnFailure()
    {
        // Arrange
        var retryPolicy = new RetryPolicy(
            maxRetries: 3,
            initialDelay: TimeSpan.FromMilliseconds(5));
        
        using var connector = new MockCloudConnector(retryPolicy: retryPolicy);
        connector.SimulateFailures = true;
        connector.FailuresBeforeSuccess = 2;

        // Act
        var items = await connector.ListItemsAsync("/test");

        // Assert - Should have retried and succeeded
        Assert.That(items, Is.Not.Null);
        Assert.That(connector.ApiCallCount, Is.GreaterThanOrEqualTo(2), "Should make at least 2 attempts");
    }

    [Test]
    public void Connector_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var connector = new MockCloudConnector();
        connector.Dispose();

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await connector.ListItemsAsync("/test"));
    }

    [Test]
    public void AuthenticationType_HasExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(AuthenticationType), AuthenticationType.OAuth2));
        Assert.That(Enum.IsDefined(typeof(AuthenticationType), AuthenticationType.ApiKey));
        Assert.That(Enum.IsDefined(typeof(AuthenticationType), AuthenticationType.Basic));
    }

    [Test]
    public void TransferProgress_CalculatesPercentCorrectly()
    {
        // Arrange
        var progress = new TransferProgress
        {
            TotalBytes = 1000,
            TransferredBytes = 250
        };

        // Assert
        Assert.That(progress.PercentComplete, Is.EqualTo(25));
    }

    [Test]
    public void QuotaInfo_CalculatesAvailableBytes()
    {
        // Arrange
        var quota = new QuotaInfo
        {
            TotalBytes = 1000,
            UsedBytes = 300
        };

        // Assert
        Assert.That(quota.AvailableBytes, Is.EqualTo(700));
    }
}
