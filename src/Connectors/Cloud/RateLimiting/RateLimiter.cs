namespace DocsUnmessed.Connectors.Cloud.RateLimiting;

/// <summary>
/// Rate limiter for API calls
/// </summary>
public sealed class RateLimiter : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly Queue<DateTime> _requestTimes;
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the RateLimiter class
    /// </summary>
    /// <param name="maxRequests">Maximum requests allowed</param>
    /// <param name="timeWindow">Time window for rate limiting</param>
    public RateLimiter(int maxRequests, TimeSpan timeWindow)
    {
        if (maxRequests <= 0)
        {
            throw new ArgumentException("Max requests must be positive", nameof(maxRequests));
        }

        if (timeWindow <= TimeSpan.Zero)
        {
            throw new ArgumentException("Time window must be positive", nameof(timeWindow));
        }

        _maxRequests = maxRequests;
        _timeWindow = timeWindow;
        _semaphore = new SemaphoreSlim(maxRequests, maxRequests);
        _requestTimes = new Queue<DateTime>();
    }

    /// <summary>
    /// Waits for rate limit clearance before proceeding
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        while (true)
        {
            await _semaphore.WaitAsync(cancellationToken);

            TimeSpan waitTime;
            bool shouldProceed = false;

            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var cutoff = now - _timeWindow;

                // Remove old requests outside the time window
                while (_requestTimes.Count > 0 && _requestTimes.Peek() < cutoff)
                {
                    _requestTimes.Dequeue();
                }

                // If we're below the limit, record this request and proceed
                if (_requestTimes.Count < _maxRequests)
                {
                    _requestTimes.Enqueue(now);
                    shouldProceed = true;
                    waitTime = TimeSpan.Zero;
                }
                else
                {
                    // At limit - calculate wait time
                    var oldestRequest = _requestTimes.Peek();
                    waitTime = (oldestRequest + _timeWindow) - now;
                    
                    if (waitTime <= TimeSpan.Zero)
                    {
                        // Window has passed, record and proceed
                        _requestTimes.Enqueue(now);
                        shouldProceed = true;
                        waitTime = TimeSpan.Zero;
                    }
                }
            }

            if (shouldProceed)
            {
                // We recorded the request, release semaphore and return
                _semaphore.Release();
                return;
            }

            // Need to wait - release semaphore and wait
            _semaphore.Release();
            
            if (waitTime > TimeSpan.Zero)
            {
                await Task.Delay(waitTime, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Gets the current request count in the time window
    /// </summary>
    public int CurrentRequestCount
    {
        get
        {
            lock (_lock)
            {
                var cutoff = DateTime.UtcNow - _timeWindow;
                while (_requestTimes.Count > 0 && _requestTimes.Peek() < cutoff)
                {
                    _requestTimes.Dequeue();
                }
                return _requestTimes.Count;
            }
        }
    }

    /// <summary>
    /// Disposes the rate limiter
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _semaphore?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RateLimiter));
        }
    }
}

/// <summary>
/// Rate limiting configuration
/// </summary>
public sealed class RateLimitConfig
{
    /// <summary>
    /// Gets or sets the maximum requests per time window
    /// </summary>
    public int MaxRequests { get; init; } = 100;

    /// <summary>
    /// Gets or sets the time window
    /// </summary>
    public TimeSpan TimeWindow { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets the default configuration
    /// </summary>
    public static RateLimitConfig Default => new();

    /// <summary>
    /// Gets a conservative configuration (slower rate)
    /// </summary>
    public static RateLimitConfig Conservative => new()
    {
        MaxRequests = 50,
        TimeWindow = TimeSpan.FromMinutes(1)
    };

    /// <summary>
    /// Gets an aggressive configuration (higher rate)
    /// </summary>
    public static RateLimitConfig Aggressive => new()
    {
        MaxRequests = 200,
        TimeWindow = TimeSpan.FromMinutes(1)
    };
}
