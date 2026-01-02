namespace DocsUnmessed.Tests.Integration.Fixtures;

using DocsUnmessed.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Test fixture for database operations
/// Provides a fresh in-memory database for each test
/// </summary>
public sealed class DatabaseFixture : IDisposable
{
    private bool _disposed;
    private readonly string _databaseName;

    /// <summary>
    /// Gets the database context for testing
    /// Note: This context should not be disposed directly if used with UnitOfWork
    /// </summary>
    public DocsUnmessedDbContext Context { get; }

    /// <summary>
    /// Initializes a new instance of the DatabaseFixture class
    /// </summary>
    public DatabaseFixture()
    {
        _databaseName = $"TestDb_{Guid.NewGuid()}";
        
        var options = new DbContextOptionsBuilder<DocsUnmessedDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        Context = new DocsUnmessedDbContext(options);
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// Creates a new isolated database context
    /// Useful for testing concurrent scenarios or when UnitOfWork will dispose the context
    /// </summary>
    /// <returns>New database context with same configuration</returns>
    public DocsUnmessedDbContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<DocsUnmessedDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        return new DocsUnmessedDbContext(options);
    }

    /// <summary>
    /// Clears all data from the database
    /// </summary>
    public void ClearDatabase()
    {
        // Use a fresh context to avoid disposal issues
        using var context = CreateNewContext();
        context.Items.RemoveRange(context.Items);
        context.Scans.RemoveRange(context.Scans);
        context.SaveChanges();
    }

    /// <summary>
    /// Disposes the database fixture
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            Context?.Database.EnsureDeleted();
            Context?.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Context may already be disposed by UnitOfWork, which is fine
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
