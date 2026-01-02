namespace DocsUnmessed.Services.Migration;

/// <summary>
/// Represents a migration operation for a single item
/// </summary>
public sealed class MigrationOperation
{
    /// <summary>
    /// Gets or sets the operation ID
    /// </summary>
    public required string OperationId { get; init; }

    /// <summary>
    /// Gets or sets the source path
    /// </summary>
    public required string SourcePath { get; init; }

    /// <summary>
    /// Gets or sets the target path
    /// </summary>
    public required string TargetPath { get; init; }

    /// <summary>
    /// Gets or sets the operation type
    /// </summary>
    public required MigrationOperationType OperationType { get; init; }

    /// <summary>
    /// Gets or sets the item size in bytes
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// Gets or sets whether this operation has conflicts
    /// </summary>
    public bool HasConflict { get; init; }

    /// <summary>
    /// Gets or sets the conflict description
    /// </summary>
    public string? ConflictDescription { get; init; }

    /// <summary>
    /// Gets or sets the rule that generated this operation
    /// </summary>
    public string? AppliedRule { get; init; }
}

/// <summary>
/// Migration operation types
/// </summary>
public enum MigrationOperationType
{
    /// <summary>
    /// Copy file to new location
    /// </summary>
    Copy,

    /// <summary>
    /// Move file to new location
    /// </summary>
    Move,

    /// <summary>
    /// Skip this item
    /// </summary>
    Skip
}

/// <summary>
/// Complete migration plan
/// </summary>
public sealed class MigrationPlan
{
    /// <summary>
    /// Gets or sets the plan ID
    /// </summary>
    public required string PlanId { get; init; }

    /// <summary>
    /// Gets or sets the scan ID this plan is for
    /// </summary>
    public required string ScanId { get; init; }

    /// <summary>
    /// Gets or sets the operations
    /// </summary>
    public required IReadOnlyList<MigrationOperation> Operations { get; init; }

    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the total size to migrate
    /// </summary>
    public long TotalSizeBytes { get; init; }

    /// <summary>
    /// Gets or sets the number of conflicts
    /// </summary>
    public int ConflictCount { get; init; }

    /// <summary>
    /// Gets or sets whether the plan is valid
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets or sets validation errors
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Migration planning configuration
/// </summary>
public sealed class MigrationPlanningConfig
{
    /// <summary>
    /// Gets or sets the target root path
    /// </summary>
    public required string TargetRootPath { get; init; }

    /// <summary>
    /// Gets or sets the default naming template
    /// </summary>
    public string DefaultNamingTemplate { get; init; } = "{Year}/{Month}/{Name}.{Extension}";

    /// <summary>
    /// Gets or sets whether to detect conflicts
    /// </summary>
    public bool DetectConflicts { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to skip duplicates
    /// </summary>
    public bool SkipDuplicates { get; init; } = true;

    /// <summary>
    /// Gets or sets the default operation type
    /// </summary>
    public MigrationOperationType DefaultOperationType { get; init; } = MigrationOperationType.Copy;

    /// <summary>
    /// Gets or sets the maximum path length
    /// </summary>
    public int MaxPathLength { get; init; } = 260; // Windows default

    /// <summary>
    /// Gets or sets whether to create target directories
    /// </summary>
    public bool CreateTargetDirectories { get; init; } = true;
}

/// <summary>
/// Conflict types
/// </summary>
public enum ConflictType
{
    /// <summary>
    /// Target path already exists
    /// </summary>
    PathExists,

    /// <summary>
    /// Path is too long
    /// </summary>
    PathTooLong,

    /// <summary>
    /// Invalid characters in path
    /// </summary>
    InvalidPath,

    /// <summary>
    /// Duplicate target path
    /// </summary>
    DuplicateTarget
}

/// <summary>
/// Represents a path conflict
/// </summary>
public sealed class PathConflict
{
    /// <summary>
    /// Gets or sets the conflicting path
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets or sets the conflict type
    /// </summary>
    public required ConflictType Type { get; init; }

    /// <summary>
    /// Gets or sets the conflict description
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets or sets the source operations causing conflict
    /// </summary>
    public IReadOnlyList<string> SourceOperations { get; init; } = Array.Empty<string>();
}
