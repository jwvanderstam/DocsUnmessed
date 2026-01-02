namespace DocsUnmessed.Data.Extensions;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Extension methods for query optimization
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    /// Adds pagination to a query
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Query to paginate</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated query</returns>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page must be >= 1", nameof(page));
        }

        if (pageSize < 1)
        {
            throw new ArgumentException("Page size must be >= 1", nameof(pageSize));
        }

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// Gets a page of results with total count
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Query to paginate</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged results with metadata</returns>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Paginate(page, pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <summary>
    /// Applies batch processing to a query
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Query to batch</param>
    /// <param name="batchSize">Batch size</param>
    /// <param name="action">Action to perform on each batch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task ProcessInBatchesAsync<T>(
        this IQueryable<T> query,
        int batchSize,
        Func<IEnumerable<T>, Task> action,
        CancellationToken cancellationToken = default)
    {
        if (batchSize < 1)
        {
            throw new ArgumentException("Batch size must be >= 1", nameof(batchSize));
        }

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var page = 1;
        bool hasMore;

        do
        {
            var batch = await query
                .Paginate(page, batchSize)
                .ToListAsync(cancellationToken);

            if (batch.Count > 0)
            {
                await action(batch);
            }

            hasMore = batch.Count == batchSize;
            page++;
        }
        while (hasMore);
    }
}

/// <summary>
/// Represents a page of results
/// </summary>
/// <typeparam name="T">Item type</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// Items in the current page
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
