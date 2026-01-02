namespace DocsUnmessed.Core.Interfaces;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Configuration;

/// <summary>
/// Abstraction for storage provider connectors
/// </summary>
public interface IConnector
{
    string Id { get; }
    ConnectorMode Mode { get; }
    
    Task AuthenticateAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<Item> EnumerateAsync(string root, EnumerationFilters? filters = null, CancellationToken cancellationToken = default);
    Task<OperationResult> OperateAsync(Operation operation, CancellationToken cancellationToken = default);
    ProviderLimits GetLimits();
    Task<bool> ValidatePathAsync(string path, CancellationToken cancellationToken = default);
}

public enum ConnectorMode
{
    Local,
    Api,
    Hybrid
}

public sealed class EnumerationFilters
{
    public string[]? Extensions { get; init; }
    public DateTime? ModifiedAfter { get; init; }
    public DateTime? ModifiedBefore { get; init; }
    public long? MinSize { get; init; }
    public long? MaxSize { get; init; }
    public int? MaxDepth { get; init; }
    public bool IncludeFolders { get; init; } = true;
    public bool ComputeHash { get; init; } = true;
    
    /// <summary>
    /// Gets or sets the exclusion configuration
    /// </summary>
    public ExcludeConfig? ExcludeConfig { get; init; }
    
    /// <summary>
    /// Gets or sets additional excluded directories (merged with ExcludeConfig)
    /// </summary>
    public string[]? ExcludedDirectories { get; init; }
    
    /// <summary>
    /// Gets or sets additional excluded file patterns (wildcards supported)
    /// </summary>
    public string[]? ExcludedFilePatterns { get; init; }
}

public sealed class OperationResult
{
    public required bool Success { get; init; }
    public required string OperationId { get; init; }
    public string? ErrorMessage { get; init; }
    public bool HashVerified { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

public sealed class ProviderLimits
{
    public required int MaxPathLength { get; init; }
    public required int MaxFileNameLength { get; init; }
    public required long MaxFileSize { get; init; }
    public required string[] InvalidCharacters { get; init; }
    public required string[] ReservedNames { get; init; }
    public int ApiRateLimitPerMinute { get; init; }
}
