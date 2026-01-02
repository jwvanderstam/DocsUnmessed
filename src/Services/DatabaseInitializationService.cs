namespace DocsUnmessed.Services;

using DocsUnmessed.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service for initializing and managing the database
/// </summary>
public sealed class DatabaseInitializationService
{
    private readonly DocsUnmessedDbContext _context;

    /// <summary>
    /// Initializes a new instance of the DatabaseInitializationService class
    /// </summary>
    /// <param name="context">Database context</param>
    public DatabaseInitializationService(DocsUnmessedDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Initializes the database by applying any pending migrations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Apply any pending migrations
            await _context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize database", ex);
        }
    }

    /// <summary>
    /// Checks if the database exists
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if database exists, false otherwise</returns>
    public async Task<bool> DatabaseExistsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.CanConnectAsync(cancellationToken);
    }

    /// <summary>
    /// Gets pending migrations that haven't been applied
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of pending migration names</returns>
    public async Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.GetPendingMigrationsAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all applied migrations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of applied migration names</returns>
    public async Task<IEnumerable<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.GetAppliedMigrationsAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes the database (use with caution!)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeleteDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureDeletedAsync(cancellationToken);
    }
}
