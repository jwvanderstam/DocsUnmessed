namespace DocsUnmessed.Data.Repositories;

using DocsUnmessed.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Generic repository implementation for basic CRUD operations
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly DocsUnmessedDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the Repository class
    /// </summary>
    /// <param name="context">Database context</param>
    public Repository(DocsUnmessedDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Func<TEntity, bool> predicate, 
        CancellationToken cancellationToken = default)
    {
        // Note: This executes client-side. For better performance, use specific repository methods with Expression<Func>
        return await Task.Run(() => _dbSet.AsNoTracking().Where(predicate).ToList(), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task UpdateAsync(TEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual Task DeleteAsync(TEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity != null;
    }
}
