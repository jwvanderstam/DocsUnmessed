namespace DocsUnmessed.Services;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Interfaces;

/// <summary>
/// Cached inventory service decorator that adds caching to another inventory service
/// </summary>
public sealed class CachedInventoryService : IInventoryService
{
    private readonly IInventoryService _inner;
    private readonly CacheService _cache;

    /// <summary>
    /// Initializes a new instance of the CachedInventoryService class
    /// </summary>
    /// <param name="inner">Inner inventory service</param>
    /// <param name="cache">Cache service</param>
    public CachedInventoryService(IInventoryService inner, CacheService cache)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc/>
    public Task<string> CreateScanAsync(string[] providers, CancellationToken cancellationToken = default)
    {
        // Don't cache scan creation
        return _inner.CreateScanAsync(providers, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(string scanId, IEnumerable<Item> items, CancellationToken cancellationToken = default)
    {
        await _inner.AddItemsAsync(scanId, items, cancellationToken);
        
        // Invalidate related caches
        _cache.Remove($"scan:{scanId}");
        _cache.Remove($"statistics:{scanId}");
        _cache.Remove($"duplicates:{scanId}");
    }

    /// <inheritdoc/>
    public Task<ScanResult> GetScanResultAsync(string scanId, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrAddAsync(
            $"scan:{scanId}",
            () => _inner.GetScanResultAsync(scanId, cancellationToken),
            expiration: TimeSpan.FromMinutes(10),
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc/>
    public Task<List<DuplicateSet>> FindDuplicatesAsync(string scanId, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrAddAsync(
            $"duplicates:{scanId}",
            () => _inner.FindDuplicatesAsync(scanId, cancellationToken),
            expiration: TimeSpan.FromMinutes(15),
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc/>
    public Task<List<ValidationIssue>> ValidateAsync(string scanId, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrAddAsync(
            $"validation:{scanId}",
            () => _inner.ValidateAsync(scanId, cancellationToken),
            expiration: TimeSpan.FromMinutes(15),
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc/>
    public Task<ScanStatistics> GetStatisticsAsync(string scanId, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrAddAsync(
            $"statistics:{scanId}",
            () => _inner.GetStatisticsAsync(scanId, cancellationToken),
            expiration: TimeSpan.FromMinutes(10),
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc/>
    public Task<IEnumerable<Item>> QueryItemsAsync(string scanId, Func<Item, bool> predicate, CancellationToken cancellationToken = default)
    {
        // Don't cache queries with custom predicates
        return _inner.QueryItemsAsync(scanId, predicate, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CompleteScanAsync(string scanId, CancellationToken cancellationToken = default)
    {
        await _inner.CompleteScanAsync(scanId, cancellationToken);
        
        // Invalidate scan cache
        _cache.Remove($"scan:{scanId}");
    }
}
