namespace DocsUnmessed.Services;

using System.Diagnostics;

/// <summary>
/// Utility for measuring query performance
/// </summary>
public sealed class PerformanceMonitor
{
    private readonly List<PerformanceMetric> _metrics = new();
    private readonly object _lock = new();

    /// <summary>
    /// Measures the execution time of an operation
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="operation">Operation to measure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    public async Task<T> MeasureAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var startMemory = GC.GetTotalMemory(false);

        try
        {
            var result = await operation();
            
            stopwatch.Stop();
            var endMemory = GC.GetTotalMemory(false);

            RecordMetric(new PerformanceMetric
            {
                OperationName = operationName,
                Duration = stopwatch.Elapsed,
                MemoryUsed = endMemory - startMemory,
                Timestamp = DateTime.UtcNow,
                Success = true
            });

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            RecordMetric(new PerformanceMetric
            {
                OperationName = operationName,
                Duration = stopwatch.Elapsed,
                MemoryUsed = 0,
                Timestamp = DateTime.UtcNow,
                Success = false,
                ErrorMessage = ex.Message
            });

            throw;
        }
    }

    /// <summary>
    /// Measures the execution time of a void operation
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="operation">Operation to measure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task MeasureAsync(
        string operationName,
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        await MeasureAsync(operationName, async () =>
        {
            await operation();
            return 0; // Dummy return value
        }, cancellationToken);
    }

    /// <summary>
    /// Gets all recorded metrics
    /// </summary>
    public IReadOnlyList<PerformanceMetric> GetMetrics()
    {
        lock (_lock)
        {
            return _metrics.ToList();
        }
    }

    /// <summary>
    /// Gets metrics for a specific operation
    /// </summary>
    /// <param name="operationName">Operation name</param>
    public IReadOnlyList<PerformanceMetric> GetMetrics(string operationName)
    {
        lock (_lock)
        {
            return _metrics
                .Where(m => m.OperationName == operationName)
                .ToList();
        }
    }

    /// <summary>
    /// Gets performance summary statistics
    /// </summary>
    public PerformanceSummary GetSummary()
    {
        lock (_lock)
        {
            if (_metrics.Count == 0)
            {
                return new PerformanceSummary
                {
                    TotalOperations = 0,
                    SuccessfulOperations = 0,
                    FailedOperations = 0,
                    AverageDuration = TimeSpan.Zero,
                    MinDuration = TimeSpan.Zero,
                    MaxDuration = TimeSpan.Zero,
                    TotalMemoryUsed = 0
                };
            }

            var successfulMetrics = _metrics.Where(m => m.Success).ToList();

            return new PerformanceSummary
            {
                TotalOperations = _metrics.Count,
                SuccessfulOperations = successfulMetrics.Count,
                FailedOperations = _metrics.Count - successfulMetrics.Count,
                AverageDuration = successfulMetrics.Any() 
                    ? TimeSpan.FromMilliseconds(successfulMetrics.Average(m => m.Duration.TotalMilliseconds))
                    : TimeSpan.Zero,
                MinDuration = successfulMetrics.Any() ? successfulMetrics.Min(m => m.Duration) : TimeSpan.Zero,
                MaxDuration = successfulMetrics.Any() ? successfulMetrics.Max(m => m.Duration) : TimeSpan.Zero,
                TotalMemoryUsed = successfulMetrics.Sum(m => m.MemoryUsed),
                OperationBreakdown = _metrics
                    .GroupBy(m => m.OperationName)
                    .ToDictionary(
                        g => g.Key,
                        g => new OperationStats
                        {
                            Count = g.Count(),
                            AverageDuration = TimeSpan.FromMilliseconds(g.Average(m => m.Duration.TotalMilliseconds)),
                            SuccessRate = g.Count(m => m.Success) / (double)g.Count() * 100
                        })
            };
        }
    }

    /// <summary>
    /// Clears all recorded metrics
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _metrics.Clear();
        }
    }

    private void RecordMetric(PerformanceMetric metric)
    {
        lock (_lock)
        {
            _metrics.Add(metric);
        }
    }
}

/// <summary>
/// Performance metric for a single operation
/// </summary>
public sealed class PerformanceMetric
{
    /// <summary>
    /// Name of the operation
    /// </summary>
    public required string OperationName { get; init; }

    /// <summary>
    /// Duration of the operation
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Memory used during the operation (in bytes)
    /// </summary>
    public long MemoryUsed { get; init; }

    /// <summary>
    /// When the operation was performed
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Whether the operation succeeded
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Performance summary statistics
/// </summary>
public sealed class PerformanceSummary
{
    /// <summary>
    /// Total number of operations measured
    /// </summary>
    public int TotalOperations { get; init; }

    /// <summary>
    /// Number of successful operations
    /// </summary>
    public int SuccessfulOperations { get; init; }

    /// <summary>
    /// Number of failed operations
    /// </summary>
    public int FailedOperations { get; init; }

    /// <summary>
    /// Average operation duration
    /// </summary>
    public TimeSpan AverageDuration { get; init; }

    /// <summary>
    /// Minimum operation duration
    /// </summary>
    public TimeSpan MinDuration { get; init; }

    /// <summary>
    /// Maximum operation duration
    /// </summary>
    public TimeSpan MaxDuration { get; init; }

    /// <summary>
    /// Total memory used across all operations
    /// </summary>
    public long TotalMemoryUsed { get; init; }

    /// <summary>
    /// Breakdown by operation name
    /// </summary>
    public Dictionary<string, OperationStats> OperationBreakdown { get; init; } = new();
}

/// <summary>
/// Statistics for a specific operation type
/// </summary>
public sealed class OperationStats
{
    /// <summary>
    /// Number of times operation was performed
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Average duration
    /// </summary>
    public TimeSpan AverageDuration { get; init; }

    /// <summary>
    /// Success rate as percentage
    /// </summary>
    public double SuccessRate { get; init; }
}
