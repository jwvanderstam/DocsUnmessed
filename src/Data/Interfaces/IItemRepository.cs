namespace DocsUnmessed.Data.Interfaces;

using DocsUnmessed.Data.Entities;

/// <summary>
/// Repository interface for item operations
/// </summary>
public interface IItemRepository : IRepository<ItemEntity>
{
    /// <summary>
    /// Gets items by scan
    /// </summary>
    /// <param name="scanId">Scan identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of items</returns>
    Task<IReadOnlyList<ItemEntity>> GetByScanAsync(string scanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets items by type
    /// </summary>
    /// <param name="scanId">Scan identifier</param>
    /// <param name="type">Item type (File or Folder)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of items</returns>
    Task<IReadOnlyList<ItemEntity>> GetByTypeAsync(string scanId, string type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets items by extension
    /// </summary>
    /// <param name="scanId">Scan identifier</param>
    /// <param name="extension">File extension</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of items</returns>
    Task<IReadOnlyList<ItemEntity>> GetByExtensionAsync(string scanId, string extension, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds potential duplicates by hash
    /// </summary>
    /// <param name="scanId">Scan identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Groups of duplicate items</returns>
    Task<IReadOnlyDictionary<string, IReadOnlyList<ItemEntity>>> FindDuplicatesAsync(string scanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets large files above a size threshold
    /// </summary>
    /// <param name="scanId">Scan identifier</param>
    /// <param name="minSizeBytes">Minimum size in bytes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of large files</returns>
    Task<IReadOnlyList<ItemEntity>> GetLargeFilesAsync(string scanId, long minSizeBytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total size of files in a scan
    /// </summary>
    /// <param name="scanId">Scan identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total size in bytes</returns>
    Task<long> GetTotalSizeAsync(string scanId, CancellationToken cancellationToken = default);
}
