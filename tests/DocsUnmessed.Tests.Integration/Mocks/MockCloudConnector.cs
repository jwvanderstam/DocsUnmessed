namespace DocsUnmessed.Tests.Integration.Mocks;

using DocsUnmessed.Connectors.Cloud;
using DocsUnmessed.Connectors.Cloud.RateLimiting;
using DocsUnmessed.Connectors.Cloud.Retry;
using DocsUnmessed.Core.Domain;

/// <summary>
/// Mock cloud connector for testing
/// </summary>
public sealed class MockCloudConnector : CloudConnectorBase
{
    private readonly Dictionary<string, Item> _items = new();
    private bool _isAuthenticated;
    private int _apiCallCount;

    /// <summary>
    /// Initializes a new instance of the MockCloudConnector class
    /// </summary>
    public MockCloudConnector(
        RateLimitConfig? rateLimitConfig = null,
        RetryPolicy? retryPolicy = null)
        : base(rateLimitConfig, retryPolicy)
    {
    }

    /// <inheritdoc/>
    public override string ProviderName => "Mock";

    /// <inheritdoc/>
    public override bool IsAuthenticated => _isAuthenticated;

    /// <summary>
    /// Gets the number of API calls made
    /// </summary>
    public int ApiCallCount => _apiCallCount;

    /// <summary>
    /// Sets whether to simulate failures
    /// </summary>
    public bool SimulateFailures { get; set; }

    /// <summary>
    /// Sets the number of failures to simulate before success
    /// </summary>
    public int FailuresBeforeSuccess { get; set; }

    private int _currentFailureCount;

    /// <inheritdoc/>
    public override Task<AuthenticationResult> AuthenticateAsync(
        CloudCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        return ExecuteApiCallAsync(async () =>
        {
            Interlocked.Increment(ref _apiCallCount);
            await Task.CompletedTask;

            if (credentials == null)
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Credentials cannot be null"
                };
            }

            _isAuthenticated = true;
            return new AuthenticationResult
            {
                IsSuccess = true,
                UserInfo = new CloudUserInfo
                {
                    UserId = "mock-user-123",
                    Email = "user@mock.com",
                    DisplayName = "Mock User",
                    Quota = new QuotaInfo
                    {
                        TotalBytes = 1024L * 1024 * 1024 * 100, // 100GB
                        UsedBytes = 1024L * 1024 * 1024 * 50    // 50GB
                    }
                },
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<IReadOnlyList<Item>> ListItemsAsync(
        string path,
        bool recursive = false,
        CancellationToken cancellationToken = default)
    {
        return ExecuteApiCallAsync(async () =>
        {
            Interlocked.Increment(ref _apiCallCount);
            await Task.CompletedTask;
            
            SimulateFailureIfNeeded();

            var items = _items.Values
                .Where(i => i.Path?.StartsWith(path) == true)
                .ToList();

            return (IReadOnlyList<Item>)items;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task DownloadFileAsync(
        string remotePath,
        string localPath,
        IProgress<TransferProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteApiCallAsync(async () =>
        {
            Interlocked.Increment(ref _apiCallCount);
            
            SimulateFailureIfNeeded();

            if (!_items.ContainsKey(remotePath))
            {
                throw new FileNotFoundException($"File not found: {remotePath}");
            }

            // Simulate download with progress
            var item = _items[remotePath];
            for (int i = 0; i <= 10; i++)
            {
                await Task.Delay(10, cancellationToken);
                progress?.Report(new TransferProgress
                {
                    TotalBytes = item.Size,
                    TransferredBytes = (item.Size * i) / 10,
                    BytesPerSecond = item.Size / 10
                });
            }
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task UploadFileAsync(
        string localPath,
        string remotePath,
        IProgress<TransferProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteApiCallAsync(async () =>
        {
            Interlocked.Increment(ref _apiCallCount);
            
            SimulateFailureIfNeeded();

            var item = new Item
            {
                Path = remotePath,
                Name = Path.GetFileName(remotePath),
                Provider = ProviderName,
                Size = 1024,
                Type = ItemType.File,
                MimeType = "application/octet-stream",
                CreatedUtc = DateTime.UtcNow,
                ModifiedUtc = DateTime.UtcNow,
                ExtendedProperties = new Dictionary<string, string>(),
                Issues = new List<string>(),
                IsShared = false,
                Depth = 0
            };

            _items[remotePath] = item;

            // Simulate upload with progress
            for (int i = 0; i <= 10; i++)
            {
                await Task.Delay(10, cancellationToken);
                progress?.Report(new TransferProgress
                {
                    TotalBytes = item.Size,
                    TransferredBytes = (item.Size * i) / 10,
                    BytesPerSecond = item.Size / 10
                });
            }
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task DeleteAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        return ExecuteApiCallAsync(async () =>
        {
            Interlocked.Increment(ref _apiCallCount);
            await Task.CompletedTask;
            
            SimulateFailureIfNeeded();

            _items.Remove(path);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<Item?> GetItemAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        return ExecuteApiCallAsync(async () =>
        {
            Interlocked.Increment(ref _apiCallCount);
            await Task.CompletedTask;
            
            SimulateFailureIfNeeded();

            return _items.TryGetValue(path, out var item) ? item : null;
        }, cancellationToken);
    }

    /// <summary>
    /// Adds a mock item for testing
    /// </summary>
    public void AddMockItem(Item item)
    {
        if (item.Path != null)
        {
            _items[item.Path] = item;
        }
    }

    private void SimulateFailureIfNeeded()
    {
        if (SimulateFailures && _currentFailureCount < FailuresBeforeSuccess)
        {
            _currentFailureCount++;
            throw new HttpRequestException("Simulated transient failure");
        }
        
        _currentFailureCount = 0;
    }
}
