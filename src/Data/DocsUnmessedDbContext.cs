namespace DocsUnmessed.Data;

using DocsUnmessed.Data.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Database context for DocsUnmessed application
/// </summary>
public sealed class DocsUnmessedDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the DocsUnmessedDbContext class
    /// </summary>
    /// <param name="options">Context options</param>
    public DocsUnmessedDbContext(DbContextOptions<DocsUnmessedDbContext> options)
        : base(options)
    {
    }

    // Entity sets
    public DbSet<ScanEntity> Scans => Set<ScanEntity>();
    public DbSet<ItemEntity> Items => Set<ItemEntity>();
    public DbSet<RuleEntity> Rules => Set<RuleEntity>();
    public DbSet<SuggestionEntity> Suggestions => Set<SuggestionEntity>();
    public DbSet<MigrationPlanEntity> MigrationPlans => Set<MigrationPlanEntity>();
    public DbSet<MigrationOperationEntity> MigrationOperations => Set<MigrationOperationEntity>();
    public DbSet<DuplicateEntity> Duplicates => Set<DuplicateEntity>();
    public DbSet<DuplicateItemEntity> DuplicateItems => Set<DuplicateItemEntity>();
    public DbSet<AuditLogEntity> AuditLog => Set<AuditLogEntity>();

    /// <summary>
    /// Configures the model for the context
    /// </summary>
    /// <param name="modelBuilder">Model builder instance</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships and constraints
        ConfigureScans(modelBuilder);
        ConfigureItems(modelBuilder);
        ConfigureRules(modelBuilder);
        ConfigureSuggestions(modelBuilder);
        ConfigureMigrationPlans(modelBuilder);
        ConfigureMigrationOperations(modelBuilder);
        ConfigureDuplicates(modelBuilder);
        ConfigureAuditLog(modelBuilder);

        // Apply global conventions
        ApplyGlobalConventions(modelBuilder);
    }

    private static void ConfigureScans(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScanEntity>(entity =>
        {
            entity.HasKey(e => e.ScanId);
            
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ProviderId);
            entity.HasIndex(e => e.StartedAt);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasMany(e => e.Items)
                .WithOne(i => i.Scan)
                .HasForeignKey(i => i.ScanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Suggestions)
                .WithOne(s => s.Scan)
                .HasForeignKey(s => s.ScanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.MigrationPlans)
                .WithOne(p => p.Scan)
                .HasForeignKey(p => p.ScanId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemEntity>(entity =>
        {
            entity.HasKey(e => e.ItemId);
            
            entity.HasIndex(e => e.ScanId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Hash).HasFilter("Hash IS NOT NULL");
            entity.HasIndex(e => e.Path);
            entity.HasIndex(e => e.Extension).HasFilter("Extension IS NOT NULL");
            entity.HasIndex(e => e.ModifiedUtc);
            entity.HasIndex(e => e.SizeBytes);

            entity.HasMany(e => e.Suggestions)
                .WithOne(s => s.Item)
                .HasForeignKey(s => s.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.DuplicateItems)
                .WithOne(d => d.Item)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRules(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RuleEntity>(entity =>
        {
            entity.HasKey(e => e.RuleId);
            
            entity.HasIndex(e => new { e.Priority, e.IsEnabled });
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasMany(e => e.Suggestions)
                .WithOne(s => s.Rule)
                .HasForeignKey(s => s.RuleId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureSuggestions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SuggestionEntity>(entity =>
        {
            entity.HasKey(e => e.SuggestionId);
            
            entity.HasIndex(e => e.ScanId);
            entity.HasIndex(e => e.ItemId);
            entity.HasIndex(e => e.RuleId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Confidence);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureMigrationPlans(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MigrationPlanEntity>(entity =>
        {
            entity.HasKey(e => e.PlanId);
            
            entity.HasIndex(e => e.ScanId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasMany(e => e.Operations)
                .WithOne(o => o.Plan)
                .HasForeignKey(o => o.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureMigrationOperations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MigrationOperationEntity>(entity =>
        {
            entity.HasKey(e => e.OperationId);
            
            entity.HasIndex(e => e.PlanId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Priority);
        });
    }

    private static void ConfigureDuplicates(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DuplicateEntity>(entity =>
        {
            entity.HasKey(e => e.DuplicateGroupId);
            
            entity.HasIndex(e => e.Hash).IsUnique();
            entity.HasIndex(e => e.FileCount);

            entity.Property(e => e.FirstSeenAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastSeenAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasMany(e => e.DuplicateItems)
                .WithOne(d => d.DuplicateGroup)
                .HasForeignKey(d => d.DuplicateGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DuplicateItemEntity>(entity =>
        {
            entity.HasKey(e => new { e.DuplicateGroupId, e.ItemId });
            
            entity.HasIndex(e => e.DuplicateGroupId);
            entity.HasIndex(e => e.ItemId);
        });
    }

    private static void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogEntity>(entity =>
        {
            entity.HasKey(e => e.LogId);
            
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.Success);

            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ApplyGlobalConventions(ModelBuilder modelBuilder)
    {
        // Configure DateTime properties to use UTC
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        v => v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
            }
        }

        // Configure string properties to trim whitespace
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string) && !property.IsNullable)
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<string, string>(
                        v => v.Trim(),
                        v => v.Trim()));
                }
            }
        }
    }

    /// <summary>
    /// Saves changes to the database with automatic timestamp updates
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities affected</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is ScanEntity scan)
            {
                scan.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is RuleEntity rule)
            {
                rule.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is MigrationPlanEntity plan)
            {
                plan.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
