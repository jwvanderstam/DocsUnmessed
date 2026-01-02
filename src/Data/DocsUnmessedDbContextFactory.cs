namespace DocsUnmessed.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Design-time factory for DocsUnmessedDbContext
/// Used by EF Core tools for migrations
/// </summary>
public sealed class DocsUnmessedDbContextFactory : IDesignTimeDbContextFactory<DocsUnmessedDbContext>
{
    /// <summary>
    /// Creates a new instance of the DbContext for design-time operations
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Configured DbContext instance</returns>
    public DocsUnmessedDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DocsUnmessedDbContext>();
        
        // Use default connection string for migrations
        optionsBuilder.UseSqlite("Data Source=docsunmessed.db");

        return new DocsUnmessedDbContext(optionsBuilder.Options);
    }
}
