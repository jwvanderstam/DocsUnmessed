namespace DocsUnmessed.Data.Interfaces;

/// <summary>
/// Generic repository interface for basic CRUD operations
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity if found, null otherwise</returns>
    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities</returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching a predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching entities</returns>
    Task<IReadOnlyList<TEntity>> FindAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">Entities to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    Task DeleteAsync(TEntity entity);

    /// <summary>
    /// Deletes multiple entities
    /// </summary>
    /// <param name="entities">Entities to delete</param>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Counts total entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
