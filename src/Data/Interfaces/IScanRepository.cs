namespace DocsUnmessed.Data.Interfaces;

using DocsUnmessed.Data.Entities;

/// <summary>
/// Repository interface for scan operations
/// </summary>
public interface IScanRepository : IRepository<ScanEntity>
{
    /// <summary>
    /// Gets scans by provider
    /// </summary>
    /// <param name="providerId">Provider identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of scans</returns>
    Task<IReadOnlyList<ScanEntity>> GetByProviderAsync(string providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets scans by status
    /// </summary>
    /// <param name="status">Scan status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of scans</returns>
    Task<IReadOnlyList<ScanEntity>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent scans
    /// </summary>
    /// <param name="count">Number of scans to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of recent scans</returns>
    Task<IReadOnlyList<ScanEntity>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets scan with items included
    /// </summary>
    /// <param name="scanId">Scan identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scan with items</returns>
    Task<ScanEntity?> GetWithItemsAsync(string scanId, CancellationToken cancellationToken = default);
}
