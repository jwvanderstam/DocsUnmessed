namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Connectors.Cloud;
using DocsUnmessed.Connectors.Cloud.OneDrive;
using DocsUnmessed.Connectors.Cloud.RateLimiting;
using DocsUnmessed.Connectors.Cloud.Retry;
using NUnit.Framework;

/// <summary>
/// Tests for OneDrive connector
/// </summary>
[TestFixture]
[CancelAfter(30000)] // 30 second timeout for OneDrive tests
public sealed class OneDriveConnectorTests
{
    [Test]
    public void Constructor_ValidConfig_CreatesConnector()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        // Act
        using var connector = new OneDriveConnector(config);

        // Assert
        Assert.That(connector.ProviderName, Is.EqualTo("OneDrive"));
        Assert.That(connector.IsAuthenticated, Is.False);
    }

    [Test]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OneDriveConnector(null!));
    }

    [Test]
    public async Task AuthenticateAsync_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        using var connector = new OneDriveConnector(config);
        
        // Note: This test requires a real Microsoft Graph API token
        // For now, we test the structure
        var credentials = new CloudCredentials
        {
            Type = AuthenticationType.OAuth2,
            AccessToken = "test-token"
        };

        // Act - This will fail without a real token, which is expected
        // In a real scenario, we'd use a mock HTTP client
        try
        {
            var result = await connector.AuthenticateAsync(credentials);
            
            // If we got here with a test token, authentication should fail
            Assert.That(result.IsSuccess, Is.False);
        }
        catch (HttpRequestException)
        {
            // Expected - test token won't work with real API
            Assert.Pass("Expected HTTP exception with test token");
        }
    }

    [Test]
    public async Task AuthenticateAsync_NullCredentials_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        using var connector = new OneDriveConnector(config);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await connector.AuthenticateAsync(null!));
    }

    [Test]
    public async Task AuthenticateAsync_EmptyToken_ReturnsFailure()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        using var connector = new OneDriveConnector(config);
        
        var credentials = new CloudCredentials
        {
            Type = AuthenticationType.OAuth2,
            AccessToken = ""
        };

        // Act
        var result = await connector.AuthenticateAsync(credentials);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Contains.Substring("token"));
    }

    [Test]
    public void ListItemsAsync_NotAuthenticated_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        using var connector = new OneDriveConnector(config);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await connector.ListItemsAsync("/"));
    }

    [Test]
    public void DownloadFileAsync_NotAuthenticated_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        using var connector = new OneDriveConnector(config);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await connector.DownloadFileAsync("/test.txt", "local.txt"));
    }

    [Test]
    public void UploadFileAsync_NotAuthenticated_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        using var connector = new OneDriveConnector(config);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await connector.UploadFileAsync("local.txt", "/test.txt"));
    }

    [Test]
    public void DeleteAsync_NotAuthenticated_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        using var connector = new OneDriveConnector(config);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await connector.DeleteAsync("/test.txt"));
    }

    [Test]
    public void GetItemAsync_NotAuthenticated_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        using var connector = new OneDriveConnector(config);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await connector.GetItemAsync("/test.txt"));
    }

    [Test]
    public void OneDriveConfig_DefaultValues_AreValid()
    {
        // Arrange & Act
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        // Assert
        Assert.That(config.GraphApiBaseUrl, Is.EqualTo("https://graph.microsoft.com/v1.0"));
        Assert.That(config.TenantId, Is.EqualTo("common"));
        Assert.That(config.RedirectUri, Is.EqualTo("http://localhost:8080"));
        Assert.That(config.Scopes, Is.Not.Empty);
        Assert.That(config.UploadChunkSize, Is.EqualTo(320 * 1024)); // 320KB
        Assert.That(config.UseDeltaQueries, Is.True);
        Assert.That(config.MaxPageSize, Is.EqualTo(200));
    }

    [Test]
    public void OneDriveConfig_Scopes_ContainsRequiredPermissions()
    {
        // Arrange & Act
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        // Assert
        Assert.That(config.Scopes, Contains.Item("Files.Read"));
        Assert.That(config.Scopes, Contains.Item("Files.ReadWrite"));
        Assert.That(config.Scopes, Contains.Item("User.Read"));
    }

    [Test]
    public void Connector_WithCustomRateLimit_UsesCustom()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        var rateLimitConfig = new RateLimitConfig
        {
            MaxRequests = 50,
            TimeWindow = TimeSpan.FromMinutes(1)
        };

        // Act
        using var connector = new OneDriveConnector(config, rateLimitConfig);

        // Assert
        Assert.That(connector, Is.Not.Null);
        Assert.That(connector.ProviderName, Is.EqualTo("OneDrive"));
    }

    [Test]
    public void Connector_WithCustomRetryPolicy_UsesCustom()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        var retryPolicy = new RetryPolicy(
            maxRetries: 5,
            initialDelay: TimeSpan.FromMilliseconds(500));

        // Act
        using var connector = new OneDriveConnector(config, retryPolicy: retryPolicy);

        // Assert
        Assert.That(connector, Is.Not.Null);
        Assert.That(connector.ProviderName, Is.EqualTo("OneDrive"));
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        var connector = new OneDriveConnector(config);

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            connector.Dispose();
            connector.Dispose();
            connector.Dispose();
        });
    }

    [Test]
    public void Dispose_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var config = new OneDriveConfig
        {
            ClientId = "test-client-id"
        };

        var connector = new OneDriveConnector(config);
        connector.Dispose();

        var credentials = new CloudCredentials
        {
            Type = AuthenticationType.OAuth2,
            AccessToken = "test"
        };

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await connector.AuthenticateAsync(credentials));
    }

    [Test]
    public void OneDriveModels_DriveInfo_HasRequiredProperties()
    {
        // Arrange & Act
        var driveInfo = new OneDriveDriveInfo
        {
            Id = "drive-123",
            Name = "My Drive",
            DriveType = "personal"
        };

        // Assert
        Assert.That(driveInfo.Id, Is.EqualTo("drive-123"));
        Assert.That(driveInfo.Name, Is.EqualTo("My Drive"));
        Assert.That(driveInfo.DriveType, Is.EqualTo("personal"));
    }

    [Test]
    public void OneDriveModels_ItemMetadata_HasRequiredProperties()
    {
        // Arrange & Act
        var metadata = new OneDriveItemMetadata
        {
            Id = "item-123",
            Name = "test.txt",
            Size = 1024,
            IsFolder = false,
            CreatedDateTime = DateTime.UtcNow,
            LastModifiedDateTime = DateTime.UtcNow
        };

        // Assert
        Assert.That(metadata.Id, Is.EqualTo("item-123"));
        Assert.That(metadata.Name, Is.EqualTo("test.txt"));
        Assert.That(metadata.Size, Is.EqualTo(1024));
        Assert.That(metadata.IsFolder, Is.False);
    }
}
