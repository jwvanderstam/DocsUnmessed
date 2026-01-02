namespace DocsUnmessed.Data;

using DocsUnmessed.Data.Interfaces;
using DocsUnmessed.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// Unit of Work implementation for managing database transactions
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DocsUnmessedDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    private IScanRepository? _scans;
    private IItemRepository? _items;

    /// <summary>
    /// Initializes a new instance of the UnitOfWork class
    /// </summary>
    /// <param name="context">Database context</param>
    public UnitOfWork(DocsUnmessedDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public IScanRepository Scans => _scans ??= new ScanRepository(_context);

    /// <inheritdoc/>
    public IItemRepository Items => _items ??= new ItemRepository(_context);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already in progress");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    /// <inheritdoc/>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    /// <summary>
    /// Disposes the unit of work and releases resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }

        _disposed = true;
    }
}
