namespace DocsUnmessed.Connectors.Cloud;

using DocsUnmessed.Connectors.Cloud.RateLimiting;
using DocsUnmessed.Connectors.Cloud.Retry;
using DocsUnmessed.Core.Domain;

/// <summary>
/// Base class for cloud connectors
/// </summary>
public abstract class CloudConnectorBase : ICloudConnector
{
    private bool _disposed;

    /// <summary>
    /// Gets the rate limiter
    /// </summary>
    protected RateLimiter RateLimiter { get; }

    /// <summary>
    /// Gets the retry policy
    /// </summary>
    protected RetryPolicy RetryPolicy { get; }

    /// <summary>
    /// Initializes a new instance of the CloudConnectorBase class
    /// </summary>
    /// <param name="rateLimitConfig">Rate limiting configuration</param>
    /// <param name="retryPolicy">Retry policy</param>
    protected CloudConnectorBase(
        RateLimitConfig? rateLimitConfig = null,
        RetryPolicy? retryPolicy = null)
    {
        var config = rateLimitConfig ?? RateLimitConfig.Default;
        RateLimiter = new RateLimiter(config.MaxRequests, config.TimeWindow);
        RetryPolicy = retryPolicy ?? Retry.RetryPolicy.Default;
    }

    /// <inheritdoc/>
    public abstract string ProviderName { get; }

    /// <inheritdoc/>
    public abstract bool IsAuthenticated { get; }

    /// <inheritdoc/>
    public abstract Task<AuthenticationResult> AuthenticateAsync(
        CloudCredentials credentials,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<IReadOnlyList<Item>> ListItemsAsync(
        string path,
        bool recursive = false,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task DownloadFileAsync(
        string remotePath,
        string localPath,
        IProgress<TransferProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task UploadFileAsync(
        string localPath,
        string remotePath,
        IProgress<TransferProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task DeleteAsync(
        string path,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<Item?> GetItemAsync(
        string path,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an API call with rate limiting and retry logic
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="apiCall">API call to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API call result</returns>
    protected async Task<T> ExecuteApiCallAsync<T>(
        Func<Task<T>> apiCall,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        // Apply rate limiting
        await RateLimiter.WaitAsync(cancellationToken);

        // Execute with retry logic
        return await RetryPolicy.ExecuteAsync(apiCall, cancellationToken);
    }

    /// <summary>
    /// Executes an API call with rate limiting and retry logic (no return value)
    /// </summary>
    /// <param name="apiCall">API call to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected async Task ExecuteApiCallAsync(
        Func<Task> apiCall,
        CancellationToken cancellationToken = default)
    {
        await ExecuteApiCallAsync(async () =>
        {
            await apiCall();
            return true;
        }, cancellationToken);
    }

    /// <summary>
    /// Disposes the connector
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the connector
    /// </summary>
    /// <param name="disposing">Whether disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            RateLimiter?.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Throws if disposed
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }
}
