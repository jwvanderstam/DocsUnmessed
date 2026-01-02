namespace DocsUnmessed.Data.Interfaces;

/// <summary>
/// Unit of Work pattern for managing database transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the scan repository
    /// </summary>
    IScanRepository Scans { get; }

    /// <summary>
    /// Gets the item repository
    /// </summary>
    IItemRepository Items { get; }

    /// <summary>
    /// Saves all pending changes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction instance</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
