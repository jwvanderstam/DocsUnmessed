namespace DocsUnmessed.Data.Repositories;

using DocsUnmessed.Data.Entities;
using DocsUnmessed.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repository implementation for scan operations
/// </summary>
public sealed class ScanRepository : Repository<ScanEntity>, IScanRepository
{
    /// <summary>
    /// Initializes a new instance of the ScanRepository class
    /// </summary>
    /// <param name="context">Database context</param>
    public ScanRepository(DocsUnmessedDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScanEntity>> GetByProviderAsync(
        string providerId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(providerId))
        {
            throw new ArgumentException("Provider ID cannot be null or empty", nameof(providerId));
        }

        return await _dbSet
            .AsNoTracking()
            .Where(s => s.ProviderId == providerId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScanEntity>> GetByStatusAsync(
        string status, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status cannot be null or empty", nameof(status));
        }

        return await _dbSet
            .AsNoTracking()
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScanEntity>> GetRecentAsync(
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            throw new ArgumentException("Count must be positive", nameof(count));
        }

        return await _dbSet
            .AsNoTracking()
            .OrderByDescending(s => s.StartedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ScanEntity?> GetWithItemsAsync(
        string scanId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scanId))
        {
            throw new ArgumentException("Scan ID cannot be null or empty", nameof(scanId));
        }

        return await _dbSet
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.ScanId == scanId, cancellationToken);
    }

    /// <summary>
    /// Gets completed scans within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of scans</returns>
    public async Task<IReadOnlyList<ScanEntity>> GetCompletedInRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.Status == "Complete" 
                && s.CompletedAt.HasValue 
                && s.CompletedAt.Value >= startDate 
                && s.CompletedAt.Value <= endDate)
            .OrderByDescending(s => s.CompletedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets scan statistics summary
    /// </summary>
    /// <param name="scanId">Scan identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scan entity or null</returns>
    public async Task<ScanEntity?> GetWithStatisticsAsync(
        string scanId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scanId))
        {
            throw new ArgumentException("Scan ID cannot be null or empty", nameof(scanId));
        }

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ScanId == scanId, cancellationToken);
    }
}
