namespace DocsUnmessed.Data.Repositories;

using DocsUnmessed.Data.Entities;
using DocsUnmessed.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repository implementation for item operations
/// </summary>
public sealed class ItemRepository : Repository<ItemEntity>, IItemRepository
{
    /// <summary>
    /// Initializes a new instance of the ItemRepository class
    /// </summary>
    /// <param name="context">Database context</param>
    public ItemRepository(DocsUnmessedDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ItemEntity>> GetByScanAsync(
        string scanId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scanId))
        {
            throw new ArgumentException("Scan ID cannot be null or empty", nameof(scanId));
        }

        return await _dbSet
            .AsNoTracking()
            .Where(i => i.ScanId == scanId)
            .OrderBy(i => i.Path)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ItemEntity>> GetByTypeAsync(
        string scanId, 
        string type, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scanId))
        {
            throw new ArgumentException("Scan ID cannot be null or empty", nameof(scanId));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Type cannot be null or empty", nameof(type));
        }

        return await _dbSet
            .AsNoTracking()
            .Where(i => i.ScanId == scanId && i.Type == type)
            .OrderBy(i => i.Path)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ItemEntity>> GetByExtensionAsync(
        string scanId, 
        string extension, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scanId))
        {
            throw new ArgumentException("Scan ID cannot be null or empty", nameof(scanId));
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("Extension cannot be null or empty", nameof(extension));
        }

        // Normalize extension (remove leading dot if present)
        var normalizedExtension = extension.TrimStart('.');

        return await _dbSet
            .AsNoTracking()
            .Where(i => i.ScanId == scanId && i.Extension == normalizedExtension)
            .OrderBy(i => i.Path)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, IReadOnlyList<ItemEntity>>> FindDuplicatesAsync(
        string scanId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scanId))
        {
            throw new ArgumentException("Scan ID cannot be null or empty", nameof(scanId));
        }

        // Find all items with non-null hashes
        var itemsWithHashes = await _dbSet
            .AsNoTracking()
            .Where(i => i.ScanId == scanId && i.Hash != null && i.Type == "File")
            .ToListAsync(cancellationToken);

        // Group by hash and filter groups with more than one item
        var duplicates = itemsWithHashes
            .GroupBy(i => i.Hash!)
            .Where(g => g.Count() > 1)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<ItemEntity>)g.OrderBy(i => i.Path).ToList()
            );

        return duplicates;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ItemEntity>> GetLargeFilesAsync(
        string scanId, 
        long minSizeBytes, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scanId))
        {
            throw new ArgumentException("Scan ID cannot be null or empty", nameof(scanId));
        }

        if (minSizeBytes < 0)
        {
            throw new ArgumentException("Minimum size cannot be negative", nameof(minSizeBytes));
        }

        return await _dbSet
            .AsNoTracking()
            .Where(i => i.ScanId == scanId && i.Type == "File" && i.SizeBytes >= minSizeBytes)
            .OrderByDescending(i => i.SizeBytes)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets items by parent path
    /// </summary>
    /// <param name="scanId">Scan identifier</param>
    /// <param name="parentPath">Parent directory path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of items</returns>
    public async Task<IReadOnlyList<ItemEntity>> GetByParentPathAsync(
        string scanId,
        string parentPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scanId))
        {
            throw new ArgumentException("Scan ID cannot be null or empty", nameof(scanId));
        }

        if (string.IsNullOrWhiteSpace(parentPath))
        {
            throw new ArgumentException("Parent path cannot be null or empty", nameof(parentPath));
        }

        return await _dbSet
            .AsNoTracking()
            .Where(i => i.ScanId == scanId && i.ParentPath == parentPath)
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets items modified after a specific date
    /// </summary>
    /// <param name="scanId">Scan identifier</param>
    /// <param name="afterDate">Date threshold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of items</returns>
    public async Task<IReadOnlyList<ItemEntity>> GetModifiedAfterAsync(
        string scanId,
        DateTime afterDate,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scanId))
        {
            throw new ArgumentException("Scan ID cannot be null or empty", nameof(scanId));
        }

        return await _dbSet
            .AsNoTracking()
            .Where(i => i.ScanId == scanId 
                && i.ModifiedUtc.HasValue 
                && i.ModifiedUtc.Value >= afterDate)
            .OrderByDescending(i => i.ModifiedUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets total size of files in a scan
    /// </summary>
    /// <param name="scanId">Scan identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total size in bytes</returns>
    public async Task<long> GetTotalSizeAsync(
        string scanId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scanId))
        {
            throw new ArgumentException("Scan ID cannot be null or empty", nameof(scanId));
        }

        return await _dbSet
            .Where(i => i.ScanId == scanId && i.Type == "File")
            .SumAsync(i => i.SizeBytes, cancellationToken);
    }
}
