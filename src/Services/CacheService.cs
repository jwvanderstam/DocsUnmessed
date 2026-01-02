namespace DocsUnmessed.Services;

using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>
/// Simple in-memory cache for frequently accessed data
/// </summary>
public sealed class CacheService : IDisposable
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly Timer _cleanupTimer;
    private readonly TimeSpan _defaultExpiration;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the CacheService class
    /// </summary>
    /// <param name="defaultExpiration">Default cache expiration time</param>
    public CacheService(TimeSpan? defaultExpiration = null)
    {
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(5);
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Gets a cached value or computes it if not present
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Factory function to compute value if not cached</param>
    /// <param name="expiration">Optional custom expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached or computed value</returns>
    public async Task<T> GetOrAddAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }

        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        // Try to get from cache
        if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
        {
            return (T)entry.Value;
        }

        // Compute value
        var value = await factory();
        
        // Store in cache
        var exp = expiration ?? _defaultExpiration;
        _cache[key] = new CacheEntry
        {
            Value = value!,
            ExpiresAt = DateTime.UtcNow.Add(exp)
        };

        return value;
    }

    /// <summary>
    /// Tries to get a value from the cache
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Output value</param>
    /// <returns>True if found and not expired</returns>
    public bool TryGet<T>(string key, out T? value)
    {
        if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
        {
            value = (T)entry.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Sets a value in the cache
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expiration">Optional custom expiration time</param>
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }

        var exp = expiration ?? _defaultExpiration;
        _cache[key] = new CacheEntry
        {
            Value = value!,
            ExpiresAt = DateTime.UtcNow.Add(exp)
        };
    }

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>True if removed</returns>
    public bool Remove(string key)
    {
        return _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// Clears all cached values
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Gets the number of items in the cache
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        var now = DateTime.UtcNow;
        var entries = _cache.Values.ToList();

        return new CacheStatistics
        {
            TotalEntries = entries.Count,
            ExpiredEntries = entries.Count(e => e.IsExpired),
            ActiveEntries = entries.Count(e => !e.IsExpired),
            OldestEntry = entries.Any() ? entries.Min(e => e.ExpiresAt) : null,
            NewestEntry = entries.Any() ? entries.Max(e => e.ExpiresAt) : null
        };
    }

    private void CleanupExpiredEntries(object? state)
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Disposes the cache service
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _cleanupTimer?.Dispose();
        _cache.Clear();
        _disposed = true;
    }

    private sealed class CacheEntry
    {
        public required object Value { get; init; }
        public DateTime ExpiresAt { get; init; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}

/// <summary>
/// Cache statistics
/// </summary>
public sealed class CacheStatistics
{
    /// <summary>
    /// Total number of entries
    /// </summary>
    public int TotalEntries { get; init; }

    /// <summary>
    /// Number of expired entries
    /// </summary>
    public int ExpiredEntries { get; init; }

    /// <summary>
    /// Number of active (non-expired) entries
    /// </summary>
    public int ActiveEntries { get; init; }

    /// <summary>
    /// Expiration time of oldest entry
    /// </summary>
    public DateTime? OldestEntry { get; init; }

    /// <summary>
    /// Expiration time of newest entry
    /// </summary>
    public DateTime? NewestEntry { get; init; }
}
