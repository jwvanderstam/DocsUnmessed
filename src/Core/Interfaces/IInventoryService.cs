namespace DocsUnmessed.Core.Interfaces;

using DocsUnmessed.Core.Domain;

/// <summary>
/// Service for storing and querying file inventory
/// </summary>
public interface IInventoryService
{
    Task<string> CreateScanAsync(string[] providers, CancellationToken cancellationToken = default);
    Task AddItemsAsync(string scanId, IEnumerable<Item> items, CancellationToken cancellationToken = default);
    Task<ScanResult> GetScanResultAsync(string scanId, CancellationToken cancellationToken = default);
    Task<List<DuplicateSet>> FindDuplicatesAsync(string scanId, CancellationToken cancellationToken = default);
    Task<List<ValidationIssue>> ValidateAsync(string scanId, CancellationToken cancellationToken = default);
    Task<ScanStatistics> GetStatisticsAsync(string scanId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> QueryItemsAsync(string scanId, Func<Item, bool> predicate, CancellationToken cancellationToken = default);
    Task CompleteScanAsync(string scanId, CancellationToken cancellationToken = default);
}
