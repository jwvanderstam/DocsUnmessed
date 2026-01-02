namespace DocsUnmessed.Connectors.Cloud.Retry;

/// <summary>
/// Retry policy for handling transient failures
/// </summary>
public sealed class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly double _backoffMultiplier;
    private readonly TimeSpan _maxDelay;

    /// <summary>
    /// Initializes a new instance of the RetryPolicy class
    /// </summary>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <param name="initialDelay">Initial delay between retries</param>
    /// <param name="backoffMultiplier">Exponential backoff multiplier</param>
    /// <param name="maxDelay">Maximum delay between retries</param>
    public RetryPolicy(
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        double backoffMultiplier = 2.0,
        TimeSpan? maxDelay = null)
    {
        if (maxRetries < 0)
        {
            throw new ArgumentException("Max retries cannot be negative", nameof(maxRetries));
        }

        if (backoffMultiplier <= 0)
        {
            throw new ArgumentException("Backoff multiplier must be positive", nameof(backoffMultiplier));
        }

        _maxRetries = maxRetries;
        _initialDelay = initialDelay ?? TimeSpan.FromSeconds(1);
        _backoffMultiplier = backoffMultiplier;
        _maxDelay = maxDelay ?? TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Executes an action with retry logic
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="action">Action to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Action result</returns>
    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var attempt = 0;
        Exception? lastException = null;

        while (attempt <= _maxRetries)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < _maxRetries)
            {
                lastException = ex;
                attempt++;

                var delay = CalculateDelay(attempt);
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw new RetryExhaustedException(
            $"Operation failed after {_maxRetries} retries",
            lastException);
    }

    /// <summary>
    /// Executes an action with retry logic (no return value)
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ExecuteAsync(
        Func<Task> action,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async () =>
        {
            await action();
            return true;
        }, cancellationToken);
    }

    private TimeSpan CalculateDelay(int attempt)
    {
        var delay = _initialDelay.TotalMilliseconds * Math.Pow(_backoffMultiplier, attempt - 1);
        var cappedDelay = Math.Min(delay, _maxDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(cappedDelay);
    }

    private static bool IsTransient(Exception exception)
    {
        // Check for common transient exceptions
        return exception is HttpRequestException
            || exception is TaskCanceledException
            || exception is TimeoutException
            || (exception is IOException ioEx && IsTransientIoException(ioEx));
    }

    private static bool IsTransientIoException(IOException exception)
    {
        // Network-related IO exceptions are typically transient
        var message = exception.Message.ToLowerInvariant();
        return message.Contains("network") 
            || message.Contains("connection") 
            || message.Contains("timeout");
    }

    /// <summary>
    /// Gets the default retry policy
    /// </summary>
    public static RetryPolicy Default => new();

    /// <summary>
    /// Gets a retry policy with no retries
    /// </summary>
    public static RetryPolicy None => new(maxRetries: 0);

    /// <summary>
    /// Gets an aggressive retry policy
    /// </summary>
    public static RetryPolicy Aggressive => new(
        maxRetries: 5,
        initialDelay: TimeSpan.FromMilliseconds(500),
        backoffMultiplier: 1.5);
}

/// <summary>
/// Exception thrown when retries are exhausted
/// </summary>
public sealed class RetryExhaustedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the RetryExhaustedException class
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public RetryExhaustedException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
